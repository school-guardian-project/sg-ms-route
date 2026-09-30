namespace ms_route.Api.Application.Dto;

public class StopResponseDto : StopListDto
{
    public Guid CityId { get; set; }
    public Guid SchoolId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
