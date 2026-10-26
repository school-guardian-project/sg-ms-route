namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for geo point.
/// </summary>
/// <param name="Latitude">Latitude.</param>
/// <param name="Longitude">Longitude.</param>
public sealed record GeoPointResponse(double Latitude, double Longitude);