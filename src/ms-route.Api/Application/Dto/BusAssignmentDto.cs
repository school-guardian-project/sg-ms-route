namespace ms_route.Api.Application.Dto;

public class BusAssignmentDto
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public Guid BusId { get; set; }
    public string Status { get; set; } = string.Empty;
}
