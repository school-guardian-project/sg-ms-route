namespace ms_route.Api.Domain.Model;

/// <summary>
/// Coordenada geográfica usada por el dominio para pedir geometría vial
/// al motor de cartografía (puerto de salida IMapRoutingService).
/// </summary>
public record GeoPoint(decimal Latitude, decimal Longitude);
