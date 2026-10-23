using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class GetRouteSuggestionUseCase : IGetRouteSuggestionUseCase
{
    private readonly IRouteRepository _routeRepository;
    private readonly IMapRoutingService _routingService;
    private readonly IRouteStopRepository _routeStopRepository;
    private readonly IStopRepository _stopRepository;

    public GetRouteSuggestionUseCase(
        IRouteRepository routeRepository,
        IMapRoutingService routingService,
        IRouteStopRepository routeStopRepository,
        IStopRepository stopRepository)
    {
        _routeRepository = routeRepository;
        _routingService = routingService;
        _routeStopRepository = routeStopRepository;
        _stopRepository = stopRepository;
    }

    public async Task<RouteSuggestionDto> ExecuteAsync(Guid routeId, CancellationToken ct = default)
    {
        // 1. Cargar ruta y paradas activas (orden ya viene por OrderSequence)
        var route = await _routeRepository.GetByIdAsync(routeId, ct)
            ?? throw new InvalidOperationException($"Route not found: {routeId}");

        var routeStops = await _routeStopRepository.GetByRouteIdAsync(routeId, ct);

        var stopIds = routeStops.Select(rs => rs.StopId).ToList();
        var stops = await _stopRepository.GetByIdsAsync(stopIds, ct);
        var stopById = stops.ToDictionary(s => s.Id);

        var orderedStops = routeStops
            .Where(rs => rs.Status == Status.Active && stopById.ContainsKey(rs.StopId))
            .Select(rs => new StopDetailDto
            {
                Id = rs.StopId,
                Name = stopById[rs.StopId]?.Name ?? string.Empty,
                Address = stopById[rs.StopId]?.Address ?? string.Empty,
                Latitude = stopById[rs.StopId]?.Latitude ?? 0,
                Longitude = stopById[rs.StopId]?.Longitude ?? 0,
                OrderSequence = rs.OrderSequence
            })
            .OrderBy(s => s.OrderSequence)
            .ToList();

        // 2. Si la ruta no tiene paradas, no hay sugerencia
        if (!orderedStops.Any())
        {
            return new RouteSuggestionDto
            {
                RouteId = routeId,
                Stops = new(),
            };
        }

        // 3. Solicitar trazado vial real (OSRM)
        var waypoints = orderedStops.Select(s => new GeoPoint(s.Latitude, s.Longitude)).ToList();
        var geometry = await _routingService.GetRouteGeometryAsync(waypoints, ct);

        // 4. Construir DTO de respuesta
        var dto = new RouteSuggestionDto
        {
            RouteId = routeId,
            Stops = orderedStops,
        };

        if (geometry is not null)
        {
            dto.Geometry = new RouteSuggestionGeometryDto
            {
                Points = geometry.Points.Select(p => new GeoPointDto((double)p.Latitude, (double)p.Longitude)).ToList(),
                DistanceMeters = geometry.DistanceMeters,
                DurationSeconds = geometry.DurationSeconds,
            };
        }

        return dto;
    }
}