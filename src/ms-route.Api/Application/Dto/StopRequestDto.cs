namespace ms_route.Api.Application.Dto;

public class StopRequestDto
{
    public string Name { get; set; } = string.Empty;
    public Guid CityId { get; set; }
    public Guid SchoolId { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal Longitude { get; set; }
    public decimal Latitude { get; set; }
}
