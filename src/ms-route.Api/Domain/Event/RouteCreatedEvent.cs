namespace ms_route.Api.Domain.Event;

public class RouteCreatedEvent
{
    public Guid EventId { get; set; }
    public Guid RouteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TargetSector { get; set; } = string.Empty;
}
