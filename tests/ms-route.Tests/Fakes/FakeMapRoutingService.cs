using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class FakeMapRoutingService : IMapRoutingService
{
    private readonly RouteGeometry? _geometry;
    private readonly bool _returnNull;

    public FakeMapRoutingService(RouteGeometry? geometry = null, bool returnNull = false)
    {
        _geometry = geometry;
        _returnNull = returnNull;
    }

    public Task<RouteGeometry?> GetRouteGeometryAsync(
        IReadOnlyList<GeoPoint> waypoints,
        CancellationToken ct = default)
    {
        if (_returnNull) return Task.FromResult<RouteGeometry?>(null);

        if (_geometry is not null) return Task.FromResult<RouteGeometry?>(_geometry);

        // Default: devuelve una línea simple entre el primer y último waypoint
        if (waypoints.Count >= 2)
        {
            var simpleGeo = new RouteGeometry
            {
                Points = new List<GeoPoint>
                {
                    waypoints[0],
                    waypoints[^1]
                },
                DistanceMeters = 1200.0,
                DurationSeconds = 600.0
            };
            return Task.FromResult<RouteGeometry?>(simpleGeo);
        }

        return Task.FromResult<RouteGeometry?>(null);
    }
}