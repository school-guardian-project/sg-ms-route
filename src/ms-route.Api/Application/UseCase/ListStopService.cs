using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class ListStopService : IListStopUseCase
{
    private readonly IStopRepository _repository;
    private readonly IRouteStopRepository _routeStopRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IMapper _mapper;

    public ListStopService(
        IStopRepository repository,
        IRouteStopRepository routeStopRepository,
        IRouteRepository routeRepository,
        IMapper mapper)
    {
        _repository = repository;
        _routeStopRepository = routeStopRepository;
        _routeRepository = routeRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StopListDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var stops = await _repository.GetAllAsync(ct);
        var result = _mapper.Map<List<StopListDto>>(stops);

        // Ruta activa de cada parada, para que el listado muestre a cual pertenece.
        var links = await _routeStopRepository.GetActiveByStopIdsAsync(result.Select(s => s.Id), ct);
        var routeNames = (await _routeRepository.GetAllAsync(null, ct)).ToDictionary(r => r.Id, r => r.Name);
        var routesByStop = links
            .Where(l => routeNames.ContainsKey(l.RouteId))
            .GroupBy(l => l.StopId)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => routeNames[l.RouteId]).Select(l => l.RouteId).Distinct().ToList());

        foreach (var stop in result)
        {
            if (!routesByStop.TryGetValue(stop.Id, out var routeIds)) continue;
            stop.RouteId = routeIds[0];
            stop.RouteName = routeNames[routeIds[0]];
            stop.RouteNames = routeIds.Select(id => routeNames[id]).ToList();
        }
        return result;
    }
}
