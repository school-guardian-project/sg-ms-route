using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class ListRouteService : IListRouteUseCase
{
    private readonly IRouteRepository _repository;
    private readonly IRouteStopRepository _routeStopRepository;
    private readonly IMapper _mapper;

    public ListRouteService(IRouteRepository repository, IRouteStopRepository routeStopRepository, IMapper mapper)
    {
        _repository = repository;
        _routeStopRepository = routeStopRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RouteListDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var routes = await _repository.GetAllAsync(ct);

        var result = _mapper.Map<List<RouteListDto>>(routes);

        var stopsCount = await _routeStopRepository.CountByRouteIdsAsync(result.Select(r => r.Id), ct);
        foreach (var route in result)
        {
            stopsCount.TryGetValue(route.Id, out var count);
            route.StopsCount = count;
        }

        return result;
    }
}
