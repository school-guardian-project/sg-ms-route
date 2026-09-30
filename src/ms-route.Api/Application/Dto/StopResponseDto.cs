namespace ms_route.Api.Application.Dto;

public class StopResponseDto
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string CityId { get; set; } = string.Empty;
    public string SchoolId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
