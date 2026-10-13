namespace ms_route.Api.Application.Dto;

public class StopListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public Guid CityId { get; set; }
    public Guid SchoolId { get; set; }
    public Guid? RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public List<string> RouteNames { get; set; } = new();
}
