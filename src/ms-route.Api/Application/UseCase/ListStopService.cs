using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class ListStopService : IListStopUseCase
{
    private readonly IStopRepository _repository;

    public ListStopService(IStopRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<StopListDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var stops = await _repository.GetAllAsync(ct);

        return stops.Select(s => new StopListDto
        {
            Id = s.Id,
            Address = s.Address,
            Latitude = s.Latitude,
            Longitude = s.Longitude
        }).ToList();
    }
}
