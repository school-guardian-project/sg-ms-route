using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class GetStopService : IGetStopUseCase
{
    private readonly IStopRepository _repository;
    private readonly IMapper _mapper;

    public GetStopService(IStopRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<StopResponseDto> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var stop = await _repository.GetByIdAsync(id, ct);
        if (stop is null)
            throw new InvalidOperationException($"Stop not found: {id}");

        return _mapper.Map<StopResponseDto>(stop);
    }
}
