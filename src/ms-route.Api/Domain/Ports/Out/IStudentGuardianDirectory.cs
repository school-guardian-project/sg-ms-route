namespace ms_route.Api.Domain.Ports.Out;

/// <summary>
/// Acudientes de un estudiante. El vinculo familiar es de ms-user-management.
/// </summary>
public interface IStudentGuardianDirectory
{
    Task<IReadOnlyCollection<Guid>> GetGuardianProfileIdsAsync(
        Guid studentProfileId, CancellationToken ct = default);
}
