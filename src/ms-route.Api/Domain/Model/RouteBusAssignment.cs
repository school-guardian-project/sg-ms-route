namespace ms_route.Api.Domain.Model;

public class RouteBusAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BusId { get; set; }
    public Guid RouteId { get; set; }
    public RouteBusAssignmentStatus Status { get; set; } = RouteBusAssignmentStatus.Active;
}

public enum RouteBusAssignmentStatus
{
    Active,
    Inactive
}
