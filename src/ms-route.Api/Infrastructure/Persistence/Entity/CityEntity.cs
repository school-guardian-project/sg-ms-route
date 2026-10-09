namespace ms_route.Api.Infrastructure.Persistence.Entity;

public class CityEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
