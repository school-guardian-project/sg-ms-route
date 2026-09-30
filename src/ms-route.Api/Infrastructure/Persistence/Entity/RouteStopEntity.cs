using ms_route.Api.Domain.Model;

namespace ms_route.Api.Infrastructure.Persistence.Entity;

public class RouteStopEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RouteId { get; set; }
    public Guid StopId { get; set; }
    public int OrderSequence { get; set; }
    public Status Status { get; set; } = Status.Active;
}
