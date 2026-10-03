using ms_route.Api.Application.Dto;
using ms_route.Api.Application.Search.Route;
using ms_route.Api.Domain.Ports.In;

namespace ms_route.Api.Application.UseCase;

public class SearchRouteService : ISearchRouteUseCase
{
    private readonly IListRouteUseCase _listUseCase;
    private readonly IEnumerable<IRouteSearchStrategy> _strategies;

    public SearchRouteService(IListRouteUseCase listUseCase, IEnumerable<IRouteSearchStrategy> strategies)
    {
        _listUseCase = listUseCase;
        _strategies = strategies;
    }

    public async Task<IEnumerable<RouteListDto>> ExecuteAsync(string search, CancellationToken ct = default)
    {
        search = search?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(search)) return [];

        var routes = await _listUseCase.ExecuteAsync(ct);

        var strategy = _strategies.FirstOrDefault(x => x.CanHandle(search));

        if (strategy == null) return [];

        return strategy.Search(search, routes);
    }
}
