namespace ms_route.Api.Application.Dto;

/// <summary>
/// Respuesta de GET /api/routes/today?driverId=.
/// El estado define si la ruta del conductor es visible en este momento:
/// - ACTIVE:    dentro de la ventana programada (15 min antes del inicio hasta el fin)
///              -> Route incluye las paradas del recorrido.
/// - SCHEDULED: aun no toca (o ya paso) -> Route sin paradas y NextOccurrence
///              con la proxima salida en hora local de Colombia (UTC-5).
/// - NO_ROUTE:  el conductor no tiene bus ni bus con ruta asignada.
/// - NO_SCHEDULE: la ruta no tiene horarios activos registrados.
/// </summary>
public class DriverRouteTodayDto
{
    public string Status { get; set; } = DriverRouteStatus.NoRoute;
    public RouteDetailDto? Route { get; set; }
    public DateTimeOffset? NextOccurrence { get; set; }
}

public static class DriverRouteStatus
{
    public const string Active = "ACTIVE";
    public const string Scheduled = "SCHEDULED";
    public const string NoRoute = "NO_ROUTE";
    public const string NoSchedule = "NO_SCHEDULE";
}
