using ms_route.Api.Application.Dto;
using ms_route.Api.Application.Search.Stop;
using ms_route.Api.Domain.Ports.In;

namespace ms_route.Api.Application.UseCase;

public class SearchStopService : ISearchStopUseCase
{
    private readonly IListStopUseCase _listUseCase;
    private readonly IEnumerable<IStopSearchStrategy> _strategies;

    public SearchStopService(IListStopUseCase listUseCase, IEnumerable<IStopSearchStrategy> strategies)
    {
        _listUseCase = listUseCase;
        _strategies = strategies;
    }

    public async Task<IEnumerable<StopListDto>> ExecuteAsync(string search, CancellationToken ct = default)
    {
        search = search?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(search)) return [];

        var stops = await _listUseCase.ExecuteAsync(ct);

        var strategy = _strategies.FirstOrDefault(x => x.CanHandle(search));

        if (strategy == null) return [];

        return strategy.Search(search, stops);
    }
}
