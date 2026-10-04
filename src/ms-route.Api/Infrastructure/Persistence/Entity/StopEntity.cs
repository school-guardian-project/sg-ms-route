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
    public Status Status { get; set; } = Status.Active;
}
