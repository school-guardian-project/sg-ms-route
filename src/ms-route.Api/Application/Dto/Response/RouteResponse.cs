namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for route with basic info.
/// </summary>
/// <param name="Id">Route identifier.</param>
/// <param name="Name">Route name.</param>
/// <param name="CampuseId">Campus identifier as string.</param>
/// <param name="TargetSector">Target sector.</param>
/// <param name="StartTime">Route start time.</param>
/// <param name="EndTime">Route end time.</param>
/// <param name="Status">Route status as string.</param>
/// <param name="BusId">Assigned bus identifier.</param>
/// <param name="Stops">List of stops.</param>
public sealed record RouteResponse(
    Guid Id,
    string Name,
    string CampuseId,
    string TargetSector,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Status,
    Guid? BusId,
    List<StopDetailResponse> Stops);