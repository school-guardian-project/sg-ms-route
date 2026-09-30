using ms_route.Api.Domain.Model;

namespace ms_route.Api.Infrastructure.Persistence.Entity;

public class RouteBusAssignmentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BusId { get; set; }
    public Guid RouteId { get; set; }
    public RouteBusAssignmentStatus Status { get; set; } = RouteBusAssignmentStatus.Active;
}
