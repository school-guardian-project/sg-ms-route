using ms_route.Api.Domain.Exceptions;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.Security;

/// <summary>
/// El conductor siempre actua como si mismo: su id sale del token, nunca del
/// body o la query (antes cualquier usuario podia iniciar o terminar el viaje
/// de otro conductor enviando su id). El staff (Admin/SuperAdmin con
/// routes.read) solo consulta.
/// </summary>
public sealed class RouteAccessPolicy : IRouteAccessPolicy
{
    private const int AdminRoleId = 1;
    private const int StudentRoleId = 2;
    private const int DriverRoleId = 3;
    private const int ParentRoleId = 4;
    private const int SuperAdminRoleId = 5;

    private const string RoutesRead = "routes.read";

    private readonly ICurrentUser _user;
    private readonly IRouteExecutionRepository _executions;
    private readonly IStudentGuardianDirectory _guardians;

    public RouteAccessPolicy(
        ICurrentUser user,
        IRouteExecutionRepository executions,
        IStudentGuardianDirectory guardians)
    {
        _user = user;
        _executions = executions;
        _guardians = guardians;
    }

    public Guid ResolveDriverId(Guid? requestedDriverId)
    {
        var profileId = RequireProfileId();

        if (_user.RoleId == DriverRoleId)
        {
            if (requestedDriverId is { } requested && requested != Guid.Empty && requested != profileId)
                throw new AccessDeniedException("A driver can only act on their own trips and routes.");

            return profileId;
        }

        if (IsStaffReader)
        {
            return requestedDriverId is { } driverId && driverId != Guid.Empty
                ? driverId
                : throw new ArgumentException("driverId is required.");
        }

        throw new AccessDeniedException();
    }

    public async Task EnsureDriverOwnsTripAsync(Guid tripId, CancellationToken ct = default)
    {
        var profileId = RequireProfileId();

        if (_user.RoleId != DriverRoleId)
            throw new AccessDeniedException("Only the driver of the trip can end it.");

        var execution = await _executions.GetByIdAsync(tripId, ct)
                        ?? throw new InvalidOperationException($"Trip not found: {tripId}");

        if (execution.DriverId != profileId)
            throw new AccessDeniedException("Only the driver of the trip can end it.");
    }

    public async Task EnsureCanReadStudentRouteAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var profileId = RequireProfileId();

        switch (_user.RoleId)
        {
            case StudentRoleId when studentProfileId == profileId:
                return;
            case ParentRoleId:
                var guardians = await _guardians.GetGuardianProfileIdsAsync(studentProfileId, ct);
                if (guardians.Contains(profileId))
                    return;
                break;
            default:
                if (IsStaffReader)
                    return;
                break;
        }

        throw new AccessDeniedException("You can only see your own route or your children's.");
    }

    // Parent y Driver tambien tienen routes.read: el permiso solo cuenta como
    // staff para Admin y SuperAdmin.
    private bool IsStaffReader =>
        _user.RoleId is AdminRoleId or SuperAdminRoleId && _user.HasPermission(RoutesRead);

    private Guid RequireProfileId() =>
        _user.IsAuthenticated && _user.ProfileId is { } id && id != Guid.Empty
            ? id
            : throw new AccessDeniedException("Authentication required.");
}
