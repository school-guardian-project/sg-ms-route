using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class EndTripService : IEndTripUseCase
{
    private readonly IRouteExecutionRepository _executionRepository;
    private readonly IMapper _mapper;

    public EndTripService(IRouteExecutionRepository executionRepository, IMapper mapper)
    {
        _executionRepository = executionRepository;
        _mapper = mapper;
    }

    public async Task<TripResponseDto> ExecuteAsync(Guid tripId, CancellationToken ct = default)
    {
        var execution = await _executionRepository.GetByIdAsync(tripId, ct);
        if (execution is null)
            throw new InvalidOperationException($"Trip not found: {tripId}");

        execution.EndDateTime = DateTime.UtcNow;
        execution.Status = Status.Inactive;

        await _executionRepository.UpdateAsync(execution, ct);

        return _mapper.Map<TripResponseDto>(execution);
    }
}
