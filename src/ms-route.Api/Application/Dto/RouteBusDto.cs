namespace ms_route.Api.Application.Dto;

/// <summary>
/// Bus de la ruta con el conductor asignado. Lo rellena ms-route desde
/// ms-fleet (gRPC) para las vistas del acudiente (FR-9.1) y del conductor (FR-9.2).
/// </summary>
public class RouteBusDto
{
    public Guid Id { get; set; }
    public string Plate { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public Guid? DriverProfileId { get; set; }
    public string DriverName { get; set; } = string.Empty;
}
