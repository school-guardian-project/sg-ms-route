using Grpc.Core;
using ms_route.Api.Application.Dto;

namespace ms_route.Api.Infrastructure.Grpc.Server;

/// <summary>
/// Conversiones puras entre los contratos protobuf (route_server.proto) y los
/// DTO/valores de la aplicacion. No tiene logica de negocio ni acceso a casos
/// de uso: RouteGrpcService sigue encargandose de invocar los casos de uso y de
/// traducir excepciones a <see cref="RpcException"/>.
/// </summary>
public static class RouteGrpcMapper
{
    /// <summary>
    /// Parsea una hora del reloj enviada por gRPC. Falla de forma explicita en
    /// lugar de devolver <see cref="TimeOnly.MinValue"/>: ese fallback silencioso
    /// guardaba 00:00:00 en Route.StartTime/EndTime, lo que desactivaba la
    /// ventana de escaneo del QR al interpretarse como "sin jornada configurada".
    /// </summary>
    public static TimeOnly ParseTime(string value)
    {
        if (TimeOnly.TryParse(value, out var time)) return time;

        throw new RpcException(new global::Grpc.Core.Status(
            StatusCode.InvalidArgument,
            $"Invalid time '{value}', expected HH:mm or HH:mm:ss"));
    }

    public static Guid ParseId(string value, string field)
    {
        if (!Guid.TryParse(value, out var id))
            throw new RpcException(new global::Grpc.Core.Status(
                StatusCode.InvalidArgument, $"Invalid {field}, use UUID: {value}"));
        return id;
    }

    /// <summary>DTO de creacion de ruta construido desde la peticion gRPC.</summary>
    public static RouteRequestDto ToRouteRequestDto(CreateRouteRequest request) => new()
    {
        CampuseId = ParseId(request.CampuseId, "campuse_id"),
        Name = request.Name,
        TargetSector = request.TargetSector,
        StartTime = ParseTime(request.StartTime),
        EndTime = ParseTime(request.EndTime)
    };

    /// <summary>DTO de actualizacion de ruta construido desde la peticion gRPC.</summary>
    public static RouteRequestDto ToRouteRequestDto(UpdateRouteRequest request) => new()
    {
        CampuseId = ParseId(request.CampuseId, "campuse_id"),
        Name = request.Name,
        TargetSector = request.TargetSector,
        StartTime = ParseTime(request.StartTime),
        EndTime = ParseTime(request.EndTime)
    };

    /// <summary>Respuesta de creacion de ruta construida desde el DTO de salida.</summary>
    public static CreateRouteResponse ToCreateRouteResponse(RouteResponseDto result) => new()
    {
        Id = result.Id.ToString(),
        Name = result.Name,
        TargetSector = result.TargetSector,
        Status = result.Status,
        StartTime = result.StartTime.ToString("HH:mm:ss"),
        EndTime = result.EndTime.ToString("HH:mm:ss")
    };

    public static IEnumerable<StopItem> ToProtoStops(IEnumerable<StopDetailDto> stops) =>
        stops.Select(s => new StopItem
        {
            Id = s.Id.ToString(),
            Name = s.Name,
            Address = s.Address,
            Latitude = (double)s.Latitude,
            Longitude = (double)s.Longitude,
            OrderSequence = s.OrderSequence
        });

    public static IEnumerable<ScheduleItem> ToProtoSchedules(IEnumerable<ScheduleDetailDto> schedules) =>
        schedules.Select(s => new ScheduleItem
        {
            DayOfWeek = s.DayOfWeek,
            Direction = s.Direction,
            StartTime = s.StartTime.ToString("HH:mm:ss")
        });
}
