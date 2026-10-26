namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for route suggestion geometry.
/// </summary>
/// <param name="Points">Points of the route in transit order.</param>
/// <param name="DistanceMeters">Distance in meters.</param>
/// <param name="DurationSeconds">Duration in seconds.</param>
public sealed record RouteSuggestionGeometryResponse(
    List<GeoPointResponse> Points,
    double DistanceMeters,
    double DurationSeconds);