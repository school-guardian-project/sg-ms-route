using ms_route.Api.Domain.Model;

namespace ms_route.Api.Infrastructure.Persistence.Entity;

public class StopEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid CityId { get; set; }
    public Guid SchoolId { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal Longitude { get; set; }
    public decimal Latitude { get; set; }
    public StopStatus Status { get; set; } = StopStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
