namespace ms_route.Api.Application.Dto;

public class RouteResponseDto : RouteListDto
{
    public Guid CampuseId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
