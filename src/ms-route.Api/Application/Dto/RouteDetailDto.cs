namespace ms_route.Api.Application.Dto;

public class RouteDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CampuseId { get; set; } = string.Empty;
    public string TargetSector { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<StopDetailDto> Stops { get; set; } = new();
    public List<ScheduleDetailDto> Schedules { get; set; } = new();

    /// <summary>Bus asignado a la ruta con su conductor (ms-fleet).</summary>
    public RouteBusDto? Bus { get; set; }
}
