using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IListStopUseCase
{
    Task<IEnumerable<StopListDto>> ExecuteAsync(CancellationToken ct = default);
}
