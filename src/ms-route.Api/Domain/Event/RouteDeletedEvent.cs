namespace ms_route.Api.Domain.Event;

public class RouteDeletedEvent
{
    public Guid EventId { get; set; }
    public Guid RouteId { get; set; }
}
