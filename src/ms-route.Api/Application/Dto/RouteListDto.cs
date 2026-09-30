namespace ms_route.Api.Application.Dto;

public class RouteListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CampuseId { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
