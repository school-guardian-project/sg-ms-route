using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class StartTripService : IStartTripUseCase
{
    private readonly IRouteExecutionRepository _executionRepository;
    private readonly IMapper _mapper;

    public StartTripService(IRouteExecutionRepository executionRepository, IMapper mapper)
    {
        _executionRepository = executionRepository;
        _mapper = mapper;
    }

    public async Task<TripResponseDto> ExecuteAsync(Guid routeId, Guid busId, Guid driverId, CancellationToken ct = default)
    {
        var execution = new RouteExecution
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            BusId = busId,
            DriverId = driverId,
            StartDateTime = DateTime.UtcNow,
            Status = Status.Active
        };

        var saved = await _executionRepository.SaveAsync(execution, ct);

        return _mapper.Map<TripResponseDto>(saved);
    }
}
