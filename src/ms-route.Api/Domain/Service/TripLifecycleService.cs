using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Port.Out;
using ms_route.Api.Domain.Service;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ms_route.Api.Domain.Service;

/// <summary>
/// Implementación del servicio de dominio para el ciclo de vida del viaje.
/// </summary>
public sealed class TripLifecycleService : ITripLifecycleService
{
    private readonly ITripBoardingQuery _boardingQuery;
    private readonly TimeProvider _timeProvider;
    private readonly TimeZoneInfo _timeZone;
    private readonly TimeSpan _defaultStartGracePeriod;
    private readonly TimeSpan _defaultNotificationBeforeStart;

    // Configuración por defecto
    public static readonly TimeSpan DefaultStartGracePeriod = TimeSpan.FromMinutes(20);
    public static readonly TimeSpan EarlyStart = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan DefaultNotificationBeforeStart = TimeSpan.FromMinutes(30);

    public TripLifecycleService(
        ITripBoardingQuery boardingQuery,
        TimeProvider timeProvider,
        TimeZoneInfo timeZone,
        TimeSpan? startGracePeriod = null,
        TimeSpan? notificationBeforeStart = null)
    {
        _boardingQuery = boardingQuery ?? throw new ArgumentNullException(nameof(boardingQuery));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        _defaultStartGracePeriod = startGracePeriod ?? DefaultStartGracePeriod;
        _defaultNotificationBeforeStart = notificationBeforeStart ?? DefaultNotificationBeforeStart;
    }

    public Task<bool> CanStartTripAsync(
        TimeOnly scheduleStartTime,
        TimeOnly localNow,
        TimeSpan? gracePeriod = null,
        CancellationToken ct = default)
    {
        return Task.FromResult(StartWindowFor(scheduleStartTime, gracePeriod).IsWithinWindow(localNow));
    }

    // La ventana abre 10 minutos antes de la hora programada y cierra tras la gracia.
    private TripStartWindow StartWindowFor(TimeOnly scheduleStartTime, TimeSpan? gracePeriod = null) =>
        TripStartWindow.Create(
            scheduleStartTime.Add(-EarlyStart),
            (gracePeriod ?? _defaultStartGracePeriod) + EarlyStart);

    public async Task<bool> CanEndTripAsync(
        Guid tripId,
        Guid routeId,
        string direction,
        CancellationToken ct = default)
    {
        var detail = await GetEndConditionDetailAsync(tripId, routeId, direction, ct);
        return detail.CanEnd;
    }

    public async Task<TripEndConditionDetail> GetEndConditionDetailAsync(
        Guid tripId,
        Guid routeId,
        string direction,
        CancellationToken ct = default)
    {
        var totalAssigned = await _boardingQuery.GetTotalAssignedStudentsAsync(routeId, ct);

        var finalStopId = await _boardingQuery.GetFinalRouteStopIdAsync(routeId, direction, ct);
        if (finalStopId is null)
        {
            return new TripEndConditionDetail(
                CanEnd: false,
                TotalAssignedStudents: totalAssigned,
                StudentsAlightedAtFinalStop: 0,
                IsFinalStopReached: false,
                FinalStopName: "No definida",
                Message: "No se encontró parada final para la ruta");
        }

        var alightedAtFinal = await _boardingQuery.GetAlightedCountAtStopAsync(tripId, finalStopId.Value, ct);
        var reachedFinal = await _boardingQuery.HasReachedFinalStopAsync(tripId, finalStopId.Value, ct);
        var stopName = await _boardingQuery.GetStopNameAsync(finalStopId.Value, ct);

        var canEnd = reachedFinal && alightedAtFinal >= totalAssigned;

        var message = canEnd
            ? "Todos los estudiantes han descendido. Puede terminar el viaje."
            : reachedFinal
                ? $"Faltan {totalAssigned - alightedAtFinal} estudiantes por descender en la parada final."
                : "El bus aún no ha llegado a la parada final.";

        return new TripEndConditionDetail(
            CanEnd: canEnd,
            TotalAssignedStudents: totalAssigned,
            StudentsAlightedAtFinalStop: alightedAtFinal,
            IsFinalStopReached: reachedFinal,
            FinalStopName: stopName,
            Message: message);
    }

    public Task<bool> ShouldNotifyBeforeStartAsync(
        TimeOnly scheduleStartTime,
        TimeOnly localNow,
        TimeSpan? notificationBefore = null,
        CancellationToken ct = default)
    {
        var notifyWindow = TripStartWindow.Create(
            scheduleStartTime.Add(-(notificationBefore ?? _defaultNotificationBeforeStart)),
            notificationBefore ?? _defaultNotificationBeforeStart);

        // Notificar si estamos en la ventana de notificación (30 min antes) pero NO en la ventana de inicio
        var inNotificationWindow = notifyWindow.IsWithinWindow(localNow);
        var inStartWindow = StartWindowFor(scheduleStartTime).IsWithinWindow(localNow);

        return Task.FromResult(inNotificationWindow && !inStartWindow);
    }
}