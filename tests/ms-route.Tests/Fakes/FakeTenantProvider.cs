using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

/// <summary>
/// Tenant por defecto sin filtro (simula llamadas sin token o SuperAdmin).
/// </summary>
public class FakeTenantProvider : ITenantProvider
{
    public int? RoleId { get; init; }
    public Guid? CampusId { get; init; }
    public Guid? SchoolId { get; init; }
    public bool ShouldFilter { get; init; }
}
