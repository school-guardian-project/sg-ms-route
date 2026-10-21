using AutoMapper;
using ms_route.Api.Application.Configuration;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class GetCurrentTripService : IGetCurrentTripUseCase
{
    private readonly IRouteExecutionRepository _executionRepository;
    private readonly IMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public GetCurrentTripService(
        IRouteExecutionRepository executionRepository,
        IMapper mapper,
        TimeProvider timeProvider)
    {
        _executionRepository = executionRepository;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<TripResponseDto?> ExecuteAsync(Guid driverId, CancellationToken ct = default)
    {
        // Solo cuenta el viaje iniciado hoy en la operacion: un viaje que nunca
        // se cerro no debe seguir devolviendose como vigente al dia siguiente.
        var execution = await _executionRepository.GetActiveByDriverAsync(
            driverId, LocalClock.StartOfLocalDayUtc(_timeProvider), ct);

        return execution is null ? null : _mapper.Map<TripResponseDto>(execution);
    }
}
