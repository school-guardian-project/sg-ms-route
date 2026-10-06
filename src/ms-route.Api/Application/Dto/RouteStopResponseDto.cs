namespace ms_route.Api.Application.Dto;

public class RouteStopResponseDto
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public Guid StopId { get; set; }
    public int OrderSequence { get; set; }
    public string Status { get; set; } = string.Empty;
}
