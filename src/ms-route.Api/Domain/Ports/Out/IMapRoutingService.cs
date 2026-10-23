using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

/// <summary>
/// Puerto de salida hacia un motor de cartografía externo (OSRM u otro).
/// El dominio solo pide "geometría vial para estos puntos"; el adaptador de
/// Infrastructure decide qué proveedor usa y qué credenciales aplica.
/// </summary>
public interface IMapRoutingService
{
    /// <summary>
    /// Trazado por calles que conecta los <paramref name="waypoints"/> en
    /// orden, o null si el proveedor no está configurado o no responde:
    /// la sugerencia es opcional y no debe tumbar el endpoint de ruta.
    /// </summary>
    Task<RouteGeometry?> GetRouteGeometryAsync(
        IReadOnlyList<GeoPoint> waypoints,
        CancellationToken ct = default);
}
