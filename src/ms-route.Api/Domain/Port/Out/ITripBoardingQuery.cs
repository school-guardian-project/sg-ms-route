using System;
using System.Threading;
using System.Threading.Tasks;

namespace ms_route.Api.Domain.Port.Out;

/// <summary>
/// Puerto de salida para consultar información de embarques/desembarques de un viaje.
/// Implementado en Infrastructure/Repository.
/// </summary>
public interface ITripBoardingQuery
{
    /// <summary>
    /// Obtiene el total de estudiantes asignados a la ruta del viaje.
    /// </summary>
    Task<int> GetTotalAssignedStudentsAsync(Guid routeId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene la cantidad de estudiantes que han descendido (OFF_BOARD) en una parada específica del viaje.
    /// </summary>
    Task<int> GetAlightedCountAtStopAsync(Guid tripId, Guid routeStopId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene la parada final del viaje según su dirección.
    /// Para "Ida": la parada con OrderSequence máximo (colegio).
    /// Para "Vuelta": la parada con OrderSequence máximo (última parada antes de base).
    /// </summary>
    Task<Guid?> GetFinalRouteStopIdAsync(Guid routeId, string direction, CancellationToken ct = default);

    /// <summary>
    /// Verifica si el bus ha llegado a la parada final (basado en geocerca/último escaneo en esa parada).
    /// </summary>
    Task<bool> HasReachedFinalStopAsync(Guid tripId, Guid finalRouteStopId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el nombre de una parada para mostrar en UI.
    /// </summary>
    Task<string> GetStopNameAsync(Guid routeStopId, CancellationToken ct = default);
}