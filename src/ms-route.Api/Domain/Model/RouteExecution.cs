namespace ms_route.Api.Domain.Model;

public class RouteExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BusId { get; set; }
    public Guid DriverId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public RouteExecutionStatus Status { get; set; } = RouteExecutionStatus.Active;
}

public enum RouteExecutionStatus
{
    Active,
    Inactive
}
