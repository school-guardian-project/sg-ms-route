namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for student route stop.
/// </summary>
/// <param name="RouteId">Route identifier.</param>
/// <param name="StudentProfileId">Student profile identifier.</param>
/// <param name="RouteStopId">Route stop identifier.</param>
/// <param name="StopId">Stop identifier.</param>
/// <param name="StopName">Stop name.</param>
/// <param name="StopAddress">Stop address.</param>
/// <param name="Latitude">Latitude.</param>
/// <param name="Longitude">Longitude.</param>
public sealed record StudentRouteStopResponse(
    Guid RouteId,
    Guid StudentProfileId,
    Guid RouteStopId,
    Guid StopId,
    string StopName,
    string StopAddress,
    decimal Latitude,
    decimal Longitude);