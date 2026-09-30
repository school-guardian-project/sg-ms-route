namespace ms_route.Api.Domain.Model;

public class RouteStop
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RouteId { get; set; }
    public Guid StopId { get; set; }
    public int OrderSequence { get; set; }
    public RouteStopStatus Status { get; set; } = RouteStopStatus.Active;
}

public enum RouteStopStatus
{
    Active,
    Inactive
}
