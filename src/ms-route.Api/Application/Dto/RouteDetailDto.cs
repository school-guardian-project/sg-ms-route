namespace ms_route.Api.Application.Dto;

public class RouteDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CampuseId { get; set; } = string.Empty;
    public string TargetSector { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<StopDetailDto> Stops { get; set; } = new();
    public List<ScheduleDetailDto> Schedules { get; set; } = new();

    /// <summary>Bus asignado a la ruta con su conductor (ms-fleet).</summary>
    public RouteBusDto? Bus { get; set; }
}

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

public class StopDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int OrderSequence { get; set; }
}

public class ScheduleDetailDto
{
    public Guid Id { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
}
