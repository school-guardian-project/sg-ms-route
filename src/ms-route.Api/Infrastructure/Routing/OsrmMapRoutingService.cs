using System.Text.Json;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Infrastructure.Routing;

/// <summary>
/// Adaptador hacia el motor OSRM (Open Source Routing Machine).
///
/// OSRM calcula el trazado por calles real (no una recta entre paradas), que
/// es lo que el mapa del conductor debe sugerir. Los waypoints llegan en el
/// orden de recogida (RouteStop.OrderSequence).
///
/// Seguridad: la URL base viene de la configuracion (Routing:BaseUrl, tipica
/// env var Routing__BaseUrl); no hay credenciales ni URL hardcodeadas. El
/// adaptador es el unico punto que conoce el proveedor: cambiar de OSRM a
/// Google/Mapbox no toca el dominio ni la aplicacion.
///
/// Resiliencia: si el proveedor no esta configurado, falla o responde mal,
/// devuelve null y el cliente sigue mostrando solo las paradas ordenadas.
/// </summary>
public sealed class OsrmMapRoutingService : IMapRoutingService, IDisposable
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(8);

    private readonly HttpClient? _client;
    private readonly string? _baseUrl;
    private readonly ILogger<OsrmMapRoutingService> _logger;

    public OsrmMapRoutingService(IConfiguration configuration, ILogger<OsrmMapRoutingService> logger)
    {
        _logger = logger;
        _baseUrl = configuration["Routing:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            // Sin configuracion no hay sugerencia, pero el endpoint no debe fallar.
            _logger.LogInformation(
                "Routing:BaseUrl no configurado; la sugerencia de recorrido se omite.");
            return;
        }

        _client = new HttpClient { Timeout = Timeout };
    }

    public async Task<RouteGeometry?> GetRouteGeometryAsync(
        IReadOnlyList<GeoPoint> waypoints,
        CancellationToken ct = default)
    {
        if (_client is null || _baseUrl is null)
            return null;

        // Una recta entre dos puntos no es un recorrido: sin al menos dos
        // paradas no hay trazado que sugerir.
        if (waypoints.Count < 2)
            return null;

        try
        {
            var path = string.Join(';', waypoints.Select(w =>
                $"{w.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                $"{w.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));

            var url = $"{_baseUrl}/route/v1/driving/{path}?overview=full&geometries=geojson";

            using var response = await _client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OSRM respondio {Status} para {Count} waypoints",
                    (int)response.StatusCode, waypoints.Count);
                return null;
            }

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            if (!doc.RootElement.TryGetProperty("routes", out var routes)
                || routes.GetArrayLength() == 0)
            {
                return null;
            }

            var best = routes[0];
            var distance = best.GetProperty("distance").GetDouble();
            var duration = best.GetProperty("duration").GetDouble();

            var coordinates = best.GetProperty("geometry").GetProperty("coordinates");
            var points = new List<GeoPoint>(coordinates.GetArrayLength());
            foreach (var coordinate in coordinates.EnumerateArray())
            {
                // GeoJSON siempre viaja [longitud, latitud].
                points.Add(new GeoPoint(
                    coordinate[1].GetDecimal(),
                    coordinate[0].GetDecimal()));
            }

            return new RouteGeometry
            {
                Points = points,
                DistanceMeters = distance,
                DurationSeconds = duration
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // La sugerencia nunca debe tumbar la vista de ruta.
            _logger.LogWarning(ex, "No se pudo obtener la geometria vial de OSRM");
            return null;
        }
    }

    public void Dispose() => _client?.Dispose();
}
