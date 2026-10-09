using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class GetCurrentTripService : IGetCurrentTripUseCase
{
    private readonly IRouteExecutionRepository _executionRepository;
    private readonly IMapper _mapper;

    public GetCurrentTripService(IRouteExecutionRepository executionRepository, IMapper mapper)
    {
        _executionRepository = executionRepository;
        _mapper = mapper;
    }

    public async Task<TripResponseDto?> ExecuteAsync(Guid driverId, CancellationToken ct = default)
    {
        var execution = await _executionRepository.GetActiveByDriverAsync(driverId, ct);

        return execution is null ? null : _mapper.Map<TripResponseDto>(execution);
    }
}
