namespace ms_route.Api.Domain.Model;

/// <summary>
/// Geometría vial real (trazado por calles) devuelta por el motor de
/// cartografía para una secuencia de waypoints. Es una sugerencia para el
/// mapa del conductor, nunca una ruta obligatoria: si el motor no responde,
/// el cliente muestra solo las paradas ordenadas.
/// </summary>
public class RouteGeometry
{
    public IReadOnlyList<GeoPoint> Points { get; init; } = Array.Empty<GeoPoint>();

    /// <summary>Distancia total del trazado en metros.</summary>
    public double DistanceMeters { get; init; }

    /// <summary>Duración estimada total en segundos.</summary>
    public double DurationSeconds { get; init; }
}
