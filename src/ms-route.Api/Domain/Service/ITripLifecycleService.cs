using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace ms_route.Api.Domain.Service;

/// <summary>
/// Servicio de dominio que encapsula la lógica del ciclo de vida de un viaje:
/// - Cuándo se puede iniciar (ventana de gracia después de StartTime)
/// - Cuándo se puede terminar (todos los estudiantes descendidos en parada final)
/// </summary>
public interface ITripLifecycleService
{
    /// <summary>
    /// Verifica si el viaje puede iniciarse ahora (dentro de la ventana de gracia).
    /// Ventana: StartTime → StartTime + GracePeriod (20 min por defecto).
    /// </summary>
    Task<bool> CanStartTripAsync(
        TimeOnly scheduleStartTime,
        TimeOnly localNow,
        TimeSpan? gracePeriod = null,
        CancellationToken ct = default);

    /// <summary>
    /// Verifica si el viaje puede terminarse ahora (todos los estudiantes descendidos en parada final).
    /// </summary>
    Task<bool> CanEndTripAsync(
        Guid tripId,
        Guid routeId,
        string direction,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene el detalle de la condición de fin de viaje para mostrar en UI.
    /// </summary>
    Task<TripEndConditionDetail> GetEndConditionDetailAsync(
        Guid tripId,
        Guid routeId,
        string direction,
        CancellationToken ct = default);

    /// <summary>
    /// Determina si se debe notificar al conductor (30 min antes de StartTime).
    /// </summary>
    Task<bool> ShouldNotifyBeforeStartAsync(
        TimeOnly scheduleStartTime,
        TimeOnly localNow,
        TimeSpan? notificationBefore = null,
        CancellationToken ct = default);
}