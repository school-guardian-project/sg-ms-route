using System;

namespace ms_route.Api.Domain.Model;

/// <summary>
/// Value object que encapsula la ventana de tiempo para iniciar un viaje.
/// Ventana: StartTime → StartTime + GracePeriod (por defecto 20 minutos).
/// </summary>
public readonly record struct TripStartWindow
{
    public TimeOnly ScheduleStartTime { get; init; }
    public TimeSpan GracePeriod { get; init; }

    public TimeOnly WindowOpensAt => ScheduleStartTime;
    public TimeOnly WindowClosesAt => ScheduleStartTime.Add(GracePeriod);

    public static TripStartWindow Create(TimeOnly scheduleStartTime, TimeSpan gracePeriod)
    {
        if (gracePeriod <= TimeSpan.Zero)
            throw new ArgumentException("Grace period must be positive", nameof(gracePeriod));

        return new TripStartWindow { ScheduleStartTime = scheduleStartTime, GracePeriod = gracePeriod };
    }

    /// <summary>
    /// Determina si la hora actual (en zona local de la operación) está dentro de la ventana de inicio.
    /// Maneja cruce de medianoche.
    /// </summary>
    public bool IsWithinWindow(TimeOnly localNow)
    {
        var opens = WindowOpensAt;
        var closes = WindowClosesAt;

        if (closes <= opens) // Cruza medianoche (ej: 23:00 - 00:20)
        {
            return localNow >= opens || localNow <= closes;
        }

        return localNow >= opens && localNow <= closes;
    }

    /// <summary>
    /// Obtiene el estado del ciclo de vida basado en la hora actual.
    /// </summary>
    public TripLifecycleStatus GetStatus(TimeOnly localNow, bool isTripStarted, bool isTripEnded)
    {
        if (isTripEnded) return TripLifecycleStatus.Ended;
        if (isTripStarted) return TripLifecycleStatus.InProgress;
        if (IsWithinWindow(localNow)) return TripLifecycleStatus.ReadyToStart;
        return TripLifecycleStatus.NotStarted;
    }
}