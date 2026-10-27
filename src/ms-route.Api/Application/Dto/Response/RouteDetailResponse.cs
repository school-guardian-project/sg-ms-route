namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for route detail (full information).
/// </summary>
/// <param name="Id">Route identifier.</param>
/// <param name="Name">Route name.</param>
/// <param name="CampuseId">Campus identifier as string.</param>
/// <param name="TargetSector">Target sector.</param>
/// <param name="StartTime">Route start time.</param>
/// <param name="EndTime">Route end time.</param>
/// <param name="Status">Route status as string.</param>
/// <param name="DaysOfWeek">Days of week.</param>
/// <param name="Stops">List of stops.</param>
/// <param name="Schedules">List of schedules.</param>
/// <param name="Bus">Assigned bus with driver info.</param>
public sealed record RouteDetailResponse(
    Guid Id,
    string Name,
    string CampuseId,
    string TargetSector,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Status,
    List<string> DaysOfWeek,
    List<StopDetailResponse> Stops,
    List<ScheduleDetailResponse> Schedules,
    RouteBusResponse? Bus = null);