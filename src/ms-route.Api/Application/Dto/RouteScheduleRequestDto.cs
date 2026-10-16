namespace ms_route.Api.Application.Dto;

public class RouteScheduleRequestDto
{
    /// <summary>Weekday name as stored by the database (Monday..Friday).</summary>
    public string DayOfWeek { get; set; } = string.Empty;

    /// <summary>Direction of the trip (e.g. "Ida" or "Vuelta"). Max 10 characters.</summary>
    public string Direction { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }
}
