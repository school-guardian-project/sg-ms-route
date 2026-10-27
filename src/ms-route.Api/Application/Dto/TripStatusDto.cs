using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Dto;

public class TripStatusDto
{
    public Guid TripId { get; set; }
    public Guid RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public TripLifecycleStatus Status { get; set; }
    public bool CanStart { get; set; }
    public bool CanEnd { get; set; }
    public TripEndConditionDetail? EndConditionDetail { get; set; }
    public TimeOnly ScheduleStartTime { get; set; }
    public TimeOnly? NextScheduleTime { get; set; }
    public string Message { get; set; } = string.Empty;
}