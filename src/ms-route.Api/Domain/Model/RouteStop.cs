namespace ms_route.Api.Domain.Model;

public class RouteStop
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RouteId { get; set; }
    public Guid StopId { get; set; }
    public int OrderSequence { get; set; }
    public Status Status { get; set; } = Status.Active;
}
