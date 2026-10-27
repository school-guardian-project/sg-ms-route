using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Domain.Service;

namespace ms_route.Api.Application.UseCase;

public class EndTripService : IEndTripUseCase
{
    private readonly IRouteExecutionRepository _executionRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IMapper _mapper;
    private readonly ITripLifecycleService _tripLifecycleService;

    public EndTripService(
        IRouteExecutionRepository executionRepository,
        IRouteRepository routeRepository,
        IMapper mapper,
        ITripLifecycleService tripLifecycleService)
    {
        _executionRepository = executionRepository;
        _routeRepository = routeRepository;
        _mapper = mapper;
        _tripLifecycleService = tripLifecycleService;
    }

    public async Task<TripResponseDto> ExecuteAsync(Guid tripId, CancellationToken ct = default)
    {
        var execution = await _executionRepository.GetByIdAsync(tripId, ct);
        if (execution is null)
            throw new InvalidOperationException($"Trip not found: {tripId}");

        if (execution.Status != Status.Active)
            throw new InvalidOperationException($"Trip {tripId} is not active (status: {execution.Status})");

        // Obtener la ruta para validar condición de fin
        var route = await _routeRepository.GetByIdAsync(execution.RouteId, ct);
        if (route is null)
            throw new InvalidOperationException($"Route {execution.RouteId} not found");

        // 1) Validar si el viaje puede terminar usando el servicio de dominio
        var canEnd = await _tripLifecycleService.CanEndTripAsync(
            tripId,
            execution.RouteId,
            execution.Direction,
            ct);

        if (!canEnd)
        {
            // Obtener detalle para mensaje de error informativo
            var detail = await _tripLifecycleService.GetEndConditionDetailAsync(
                tripId,
                execution.RouteId,
                execution.Direction,
                ct);

            throw new InvalidOperationException(
                $"Cannot end trip: {detail.Message} " +
                $"(Descendidos: {detail.StudentsAlightedAtFinalStop}/{detail.TotalAssignedStudents} en '{detail.FinalStopName}')");
        }

        // 2) Terminar el viaje
        execution.EndDateTime = DateTime.UtcNow;
        execution.Status = Status.Inactive;

        await _executionRepository.UpdateAsync(execution, ct);

        return _mapper.Map<TripResponseDto>(execution);
    }
}