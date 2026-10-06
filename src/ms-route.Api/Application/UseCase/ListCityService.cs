using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class ListCityService : IListCityUseCase
{
    private readonly ICityRepository _repository;

    public ListCityService(ICityRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CityListDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var cities = await _repository.GetAllAsync(ct);

        return cities.Select(c => new CityListDto { Id = c.Id, Name = c.Name }).ToList();
    }
}
