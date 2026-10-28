using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Service;

/// <summary>
/// Servicio de dominio dueño de la regla de horarios de una ruta: los horarios
/// cuelgan de la asignacion bus-ruta y sin horario del dia el conductor no puede
/// iniciar el viaje. Lo usan la asignacion del bus, la edicion de la ruta y el
/// alta manual de horarios, para que la regla viva en un solo lugar.
/// </summary>
public interface IRouteScheduleProvisioner
{
    /// <summary>
    /// Crea los horarios Ida/Vuelta que falten para cada dia con la hora de salida
    /// indicada. Los que ya existen se omiten: repetir la llamada no duplica.
    /// </summary>
    /// <returns>Solo los horarios creados en esta llamada.</returns>
    Task<IReadOnlyList<RouteSchedule>> EnsureSchedulesAsync(
        Guid routeBusAssignmentId,
        TimeOnly startTime,
        IEnumerable<DayOfWeek> days,
        CancellationToken ct = default);

    /// <summary>
    /// Alinea la hora de salida de los horarios activos de la asignacion con la de
    /// la ruta, para que la ventana de inicio no siga usando una hora vieja.
    /// </summary>
    Task SyncStartTimeAsync(
        Guid routeBusAssignmentId,
        TimeOnly startTime,
        CancellationToken ct = default);
}
