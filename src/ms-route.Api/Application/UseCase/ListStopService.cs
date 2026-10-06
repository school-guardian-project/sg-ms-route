using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class ListStopService : IListStopUseCase
{
    private readonly IStopRepository _repository;
    private readonly IMapper _mapper;

    public ListStopService(IStopRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StopListDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var stops = await _repository.GetAllAsync(ct);

        return _mapper.Map<IEnumerable<StopListDto>>(stops);
    }
}
