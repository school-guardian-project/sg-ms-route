namespace ms_route.Api.Application.Dto;

public class StudentRouteStopDto
{
    public Guid RouteId { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid RouteStopId { get; set; }
    public Guid StopId { get; set; }
    public string StopName { get; set; } = string.Empty;
}
