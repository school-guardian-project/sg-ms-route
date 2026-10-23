namespace ms_route.Api.Application.Dto;

public class ScheduleDetailDto
{
    public Guid Id { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
}
