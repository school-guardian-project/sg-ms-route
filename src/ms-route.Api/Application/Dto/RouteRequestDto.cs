namespace ms_route.Api.Application.Dto;

public class RouteRequestDto
{
    public Guid CampuseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TargetSector { get; set; } = string.Empty;
}
