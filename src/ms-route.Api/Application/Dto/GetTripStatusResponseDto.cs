using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Dto;

/// <summary>
/// DTO para la respuesta de GetTripStatus.
/// </summary>
public class GetTripStatusResponseDto
{
    public bool Found { get; set; }
    public Guid? TripId { get; set; }
    public Guid RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public TripLifecycleStatus Status { get; set; }
    public bool CanStart { get; set; }
    public bool CanEnd { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeOnly ScheduleStartTime { get; set; }
    public TimeOnly? NextScheduleTime { get; set; }
    public TripEndConditionDetailDto? EndConditionDetail { get; set; }
}

/// <summary>
/// DTO para el detalle de condición de fin de viaje.
/// </summary>
public class TripEndConditionDetailDto
{
    public bool CanEnd { get; set; }
    public int TotalAssignedStudents { get; set; }
    public int StudentsAlightedAtFinalStop { get; set; }
    public bool IsFinalStopReached { get; set; }
    public string FinalStopName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}