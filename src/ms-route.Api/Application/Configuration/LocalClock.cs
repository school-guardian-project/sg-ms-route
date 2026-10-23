namespace ms_route.Api.Application.Configuration;

/// <summary>
/// Fuente unica de la hora local de la operacion en ms-route.
///
/// Se alinea con <c>ScanWindowOptions.TimeZoneId</c> de ms-notification
/// (America/Bogota) para que la ventana activa del conductor y la ventana de
/// escaneo del QR compartan la misma zona horaria. Antes ms-route fijaba el
/// offset UTC-5 en duro y ms-notification resolvia la zona IANA: dos
/// implementaciones del mismo offset que podian divergir en silencio.
///
/// Uso: las columnas SQL <c>time</c> de Route/RouteSchedule son hora del reloj
/// local sin zona; las columnas <c>datetime2</c> de RouteExecution se escriben
/// en UTC. Este tipo es la unica bisagra entre ambos relojes.
/// </summary>
public static class LocalClock
{
    private const string TimeZoneId = "America/Bogota";

    // Colombia no aplica horario de verano: offset fijo UTC-5.
    private static readonly TimeSpan FallbackOffset = TimeSpan.FromHours(-5);

    public static TimeZoneInfo TimeZone { get; } = Resolve();

    /// <summary>Ahora en hora local de la operacion.</summary>
    public static DateTimeOffset UtcNow(TimeProvider timeProvider) =>
        TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), TimeZone);

    /// <summary>
    /// Compone un instante local a partir de una fecha y una hora del reloj,
    /// tal como vienen de las columnas SQL <c>time</c> (sin zona horaria).
    /// </summary>
    public static DateTimeOffset AtLocal(DateOnly date, TimeOnly time)
    {
        var local = date.ToDateTime(time);
        return new DateTimeOffset(local, TimeZone.GetUtcOffset(local));
    }

    /// <summary>
    /// Inicio del dia local (00:00 de la operacion) expresado en UTC, para
    /// compararlo contra columnas <c>datetime2</c> escritas con
    /// <c>DateTime.UtcNow</c>.
    /// </summary>
    public static DateTime StartOfLocalDayUtc(TimeProvider timeProvider)
    {
        var today = DateOnly.FromDateTime(UtcNow(timeProvider).DateTime);
        return TimeZoneInfo.ConvertTimeToUtc(today.ToDateTime(TimeOnly.MinValue), TimeZone);
    }

    private static TimeZoneInfo Resolve()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            // Imagen sin tzdata: se degrada al offset fijo de Colombia.
            return TimeZoneInfo.CreateCustomTimeZone(
                TimeZoneId, FallbackOffset, TimeZoneId, TimeZoneId);
        }
    }
}
