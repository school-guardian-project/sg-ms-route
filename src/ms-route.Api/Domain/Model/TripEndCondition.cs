namespace ms_route.Api.Domain.Model;

/// <summary>
/// Value object que encapsula la condición para terminar un viaje.
/// Un viaje puede terminar cuando:
/// - Dirección "Ida": todos los estudiantes asignados han descendido en la parada del colegio (OrderSequence máximo)
/// - Dirección "Vuelta": todos los estudiantes asignados han descendido en la última parada
/// </summary>
public readonly record struct TripEndCondition
{
    public string Direction { get; init; } // "Ida" o "Vuelta"
    public int TotalAssignedStudents { get; init; }
    public int StudentsAlightedAtFinalStop { get; init; }
    public bool IsFinalStopReached { get; init; }

    public bool CanEndTrip => IsFinalStopReached && StudentsAlightedAtFinalStop >= TotalAssignedStudents;

    public static TripEndCondition Create(
        string direction,
        int totalAssignedStudents,
        int studentsAlightedAtFinalStop,
        bool isFinalStopReached)
    {
        return new TripEndCondition
        {
            Direction = direction,
            TotalAssignedStudents = totalAssignedStudents,
            StudentsAlightedAtFinalStop = studentsAlightedAtFinalStop,
            IsFinalStopReached = isFinalStopReached
        };
    }
}