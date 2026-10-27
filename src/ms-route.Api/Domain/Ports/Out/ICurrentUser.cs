namespace ms_route.Api.Domain.Ports.Out;

/// <summary>
/// Identidad del usuario autenticado segun el access token validado.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>Perfil autenticado (claim "sub").</summary>
    Guid? ProfileId { get; }

    int? RoleId { get; }

    bool HasPermission(string permission);
}
