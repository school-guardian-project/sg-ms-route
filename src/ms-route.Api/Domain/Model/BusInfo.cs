namespace ms_route.Api.Domain.Model;

/// <summary>
/// Bus con su conductor tal y como lo devuelve ms-fleet. Lo usan las vistas de
/// ruta del acudiente (FR-9.1) y del conductor (FR-9.2): placa y conductor son
/// datos de ms-fleet, nunca se leen de su esquema desde aqui (ADR-005).
/// </summary>
public class BusInfo
{
    public Guid Id { get; set; }
    public string Plate { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public Guid? DriverProfileId { get; set; }
    public string DriverName { get; set; } = string.Empty;
}
