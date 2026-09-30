using ms_route.Api.Domain.Model;

namespace ms_route.Api.Infrastructure.Persistence.Entity;

public class RouteEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CampuseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TargetSector { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public RouteStatus Status { get; set; } = RouteStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
