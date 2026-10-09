using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface ISearchStopUseCase
{
    Task<IEnumerable<StopListDto>> ExecuteAsync(string search, CancellationToken ct = default);
}
