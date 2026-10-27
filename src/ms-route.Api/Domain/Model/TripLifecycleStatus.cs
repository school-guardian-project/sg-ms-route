namespace ms_route.Api.Domain.Model;

/// <summary>
/// Estado del ciclo de vida de un viaje (RouteExecution).
/// </summary>
public enum TripLifecycleStatus
{
    /// <summary>El viaje no ha iniciado y está fuera de la ventana de gracia.</summary>
    NotStarted,

    /// <summary>El viaje está en la ventana de gracia para iniciar (StartTime a StartTime + GracePeriod).</summary>
    ReadyToStart,

    /// <summary>El viaje está activo (iniciado).</summary>
    InProgress,

    /// <summary>Todos los estudiantes han descendido en la parada final - listo para terminar.</summary>
    ReadyToEnd,

    /// <summary>El viaje ha finalizado.</summary>
    Ended
}