namespace ms_route.Api.Domain.Model;

public class RouteStudentAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public Guid RouteStopId { get; set; }
    public RouteStudentAssignmentStatus Status { get; set; } = RouteStudentAssignmentStatus.Active;
}

public enum RouteStudentAssignmentStatus
{
    Active,
    Inactive
}
