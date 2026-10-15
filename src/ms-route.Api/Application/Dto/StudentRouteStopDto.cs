namespace ms_route.Api.Application.Dto;

public class StudentRouteStopDto
{
    public Guid RouteId { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid RouteStopId { get; set; }
    public Guid StopId { get; set; }
    public string StopName { get; set; } = string.Empty;

    // Ubicacion de la parada asignada: la direccion para el texto de la
    // notificacion y las coordenadas para ubicarla en un mapa (FR-5.2, FR-5.3).
    public string StopAddress { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
