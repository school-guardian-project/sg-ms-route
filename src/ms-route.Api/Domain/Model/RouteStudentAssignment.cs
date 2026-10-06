namespace ms_route.Api.Domain.Model;

public class RouteStudentAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public Guid RouteStopId { get; set; }
    public Status Status { get; set; } = Status.Active;
}
