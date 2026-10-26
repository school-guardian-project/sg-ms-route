using ms_route.Api.Application.Dto.Response;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps RouteSuggestion domain entities to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface IRouteSuggestionMapper
{
    RouteSuggestionResponse ToResponse(
        Guid routeId,
        List<StopDetailResponse> stops,
        RouteSuggestionGeometryResponse? geometry);
    RouteSuggestionGeometryResponse ToGeometryResponse(
        List<GeoPointResponse> points,
        double distanceMeters,
        double durationSeconds);
    GeoPointResponse ToGeoPointResponse(GeoPoint point);
}

public sealed class RouteSuggestionMapper : IRouteSuggestionMapper
{
    public RouteSuggestionResponse ToResponse(
        Guid routeId,
        List<StopDetailResponse> stops,
        RouteSuggestionGeometryResponse? geometry)
    {
        return new RouteSuggestionResponse(routeId, stops, geometry);
    }

    public RouteSuggestionGeometryResponse ToGeometryResponse(
        List<GeoPointResponse> points,
        double distanceMeters,
        double durationSeconds)
    {
        return new RouteSuggestionGeometryResponse(points, distanceMeters, durationSeconds);
    }

    public GeoPointResponse ToGeoPointResponse(GeoPoint point)
    {
        return new GeoPointResponse(point.Latitude, point.Longitude);
    }
}