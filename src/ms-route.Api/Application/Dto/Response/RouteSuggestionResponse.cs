namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for route suggestion.
/// </summary>
/// <param name="RouteId">Route identifier.</param>
/// <param name="Stops">Stops in pickup order.</param>
/// <param name="Geometry">Route geometry.</param>
public sealed record RouteSuggestionResponse(
    Guid RouteId,
    List<StopDetailResponse> Stops,
    RouteSuggestionGeometryResponse? Geometry);