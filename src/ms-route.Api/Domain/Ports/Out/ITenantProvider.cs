namespace ms_route.Api.Domain.Ports.Out;

/// <summary>
/// Expone el tenant (colegio/sede) del usuario autenticado según los claims del JWT.
/// </summary>
public interface ITenantProvider
{
    int? RoleId { get; }
    Guid? CampusId { get; }
    Guid? SchoolId { get; }

    /// <summary>
    /// true cuando el token es válido y el rol exige acotar las rutas por tenant.
    /// Sin token, token inválido o SuperAdmin (roleId 5) => false.
    /// </summary>
    bool ShouldFilter { get; }
}
