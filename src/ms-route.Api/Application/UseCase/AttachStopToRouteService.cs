using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class AttachStopToRouteService : IAttachStopToRouteUseCase
{
    private readonly IRouteRepository _routeRepository;
    private readonly IStopRepository _stopRepository;
    private readonly IRouteStopRepository _routeStopRepository;

    public AttachStopToRouteService(
        IRouteRepository routeRepository,
        IStopRepository stopRepository,
        IRouteStopRepository routeStopRepository)
    {
        _routeRepository = routeRepository;
        _stopRepository = stopRepository;
        _routeStopRepository = routeStopRepository;
    }

    public async Task<RouteStopResponseDto> ExecuteAsync(Guid routeId, Guid stopId, CancellationToken ct = default)
    {
        var route = await _routeRepository.GetByIdAsync(routeId, ct);
        if (route is null)
            throw new InvalidOperationException($"Route not found: {routeId}");

        var stop = await _stopRepository.GetByIdAsync(stopId, ct);
        if (stop is null)
            throw new InvalidOperationException($"Stop not found: {stopId}");

        var routeStops = await _routeStopRepository.GetByRouteIdAsync(routeId, ct);
        if (routeStops.Any(rs => rs.StopId == stopId))
            throw new InvalidOperationException("Stop already on route");

        var routeStop = new RouteStop
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            StopId = stopId,
            OrderSequence = routeStops.Count == 0 ? 1 : routeStops.Max(rs => rs.OrderSequence) + 1,
            Status = Status.Active
        };

        var saved = await _routeStopRepository.SaveAsync(routeStop, ct);

        return new RouteStopResponseDto
        {
            Id = saved.Id,
            RouteId = saved.RouteId,
            StopId = saved.StopId,
            OrderSequence = saved.OrderSequence,
            Status = saved.Status.ToString()
        };
    }
}
