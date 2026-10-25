namespace ms_route.Api.Application.Dto;

/// <summary>
/// Respuesta de GET /api/routes/{id}/suggestion: sugerencia de recorrido para
/// el mapa del conductor (FR-9.2).
///
/// Es una sugerencia, no una obligacion: el cliente la dibuja como guia
/// visual. Si Geometry es null (proveedor sin configurado o caido), el mapa
/// sigue mostrando las paradas ordenadas por OrderSequence como hasta ahora.
/// </summary>
public class RouteSuggestionDto
{
    public Guid RouteId { get; set; }

    /// <summary>Paradas en orden de recogida (OrderSequence asc).</summary>
    public List<StopDetailDto> Stops { get; set; } = new();

    /// <summary>Trazado por calles o null si no hay sugerencia disponible.</summary>
    public RouteSuggestionGeometryDto? Geometry { get; set; }

    /// <summary>
    /// Propiedad de conveniencia para compatibilidad con mobile.
    /// Devuelve los puntos del trazado como array plano (camelCase al serializar).
    /// Mobile puede usar `suggestion.points` directamente sin acceder a `geometry.Points`.
    /// </summary>
    public List<GeoPointDto> Points => Geometry?.Points ?? new();
}
