namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for driver route today.
/// </summary>
/// <param name="Status">Route status.</param>
/// <param name="Route">Route detail if active.</param>
/// <param name="NextOccurrence">Next scheduled occurrence.</param>
public sealed record DriverRouteTodayResponse(
    string Status,
    RouteDetailResponse? Route,
    DateTimeOffset? NextOccurrence);