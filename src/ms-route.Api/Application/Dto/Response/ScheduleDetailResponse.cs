namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for schedule detail.
/// </summary>
/// <param name="Id">Schedule identifier.</param>
/// <param name="DayOfWeek">Day of week.</param>
/// <param name="Direction">Direction.</param>
/// <param name="StartTime">Start time.</param>
public sealed record ScheduleDetailResponse(
    Guid Id,
    string DayOfWeek,
    string Direction,
    TimeOnly StartTime);