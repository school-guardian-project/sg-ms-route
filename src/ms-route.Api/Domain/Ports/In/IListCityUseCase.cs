using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IListCityUseCase
{
    Task<IEnumerable<CityListDto>> ExecuteAsync(CancellationToken ct = default);
}
