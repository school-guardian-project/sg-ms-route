using System.Security.Claims;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Infrastructure.Auth;

/// <summary>
/// <see cref="ICurrentUser"/> a partir de los claims del access token validado.
/// </summary>
public sealed class ClaimsCurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal? _user;

    public ClaimsCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated == true;

    public Guid? ProfileId =>
        Guid.TryParse(GetClaim(SgClaimTypes.Subject), out var id) ? id : null;

    public int? RoleId =>
        int.TryParse(GetClaim(SgClaimTypes.RoleId), out var role) ? role : null;

    public bool HasPermission(string permission) =>
        IsAuthenticated && _user!.HasClaim(SgClaimTypes.Permissions, permission);

    private string? GetClaim(string type) =>
        IsAuthenticated ? _user!.FindFirst(type)?.Value : null;
}
