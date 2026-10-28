namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for route list item (minimal info for listings).
/// </summary>
/// <param name="Id">Route identifier.</param>
/// <param name="Name">Route name.</param>
/// <param name="CampuseId">Campus identifier as string.</param>
/// <param name="TargetSector">Target sector.</param>
/// <param name="StartTime">Route start time.</param>
/// <param name="EndTime">Route end time.</param>
/// <param name="DaysOfWeek">Configured workdays.</param>
/// <param name="StopsCount">Number of stops.</param>
public sealed record RouteListResponse(
    Guid Id,
    string Name,
    string CampuseId,
    string TargetSector,
    TimeOnly StartTime,
    TimeOnly EndTime,
    List<string> DaysOfWeek,
    int StopsCount);