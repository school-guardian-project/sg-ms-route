namespace ms_route.Api.Application.Dto;

public class StopListDto
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal Longitude { get; set; }
    public decimal Latitude { get; set; }
    public string Status { get; set; } = string.Empty;
}
