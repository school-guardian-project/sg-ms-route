namespace ms_route.Api.Application.Dto;

/// <summary>
/// Detalle de la condición de fin de viaje para UI.
/// </summary>
public record TripEndConditionDetail(
    bool CanEnd,
    int TotalAssignedStudents,
    int StudentsAlightedAtFinalStop,
    bool IsFinalStopReached,
    string FinalStopName,
    string Message);