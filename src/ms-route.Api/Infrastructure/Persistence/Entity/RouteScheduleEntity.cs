using ms_route.Api.Domain.Model;

namespace ms_route.Api.Infrastructure.Persistence.Entity;

public class RouteScheduleEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RouteBusAssignmentsId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string Direction { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public RouteScheduleStatus Status { get; set; } = RouteScheduleStatus.Active;
}
