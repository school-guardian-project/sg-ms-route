using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Domain.Service;

namespace ms_route.Api.Application.UseCase;

public class AssignBusToRouteService : IAssignBusToRouteUseCase
{
    private readonly IRouteBusAssignmentRepository _assignmentRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IRouteScheduleProvisioner _scheduleProvisioner;
    private readonly IMapper _mapper;

    public AssignBusToRouteService(
        IRouteBusAssignmentRepository assignmentRepository,
        IRouteRepository routeRepository,
        IRouteScheduleProvisioner scheduleProvisioner,
        IMapper mapper)
    {
        _assignmentRepository = assignmentRepository;
        _routeRepository = routeRepository;
        _scheduleProvisioner = scheduleProvisioner;
        _mapper = mapper;
    }

    public async Task<BusAssignmentDto> ExecuteAsync(Guid routeId, Guid busId, CancellationToken ct = default)
    {
        var route = await _routeRepository.GetByIdAsync(routeId, ct);
        if (route is null || route.Status != Status.Active)
            throw new InvalidOperationException($"Route not found: {routeId}");

        var routeDays = RouteScheduleProvisioner.ParseConfiguredDays(route.DaysOfWeek);

        // Un bus por ruta.
        if (await _assignmentRepository.RouteHasBusAsync(routeId, ct))
            throw new InvalidOperationException("The route already has a bus assigned");

        // Una ruta por bus.
        var currentAssignment = await _assignmentRepository.GetActiveByBusIdAsync(busId, ct);
        if (currentAssignment is not null && currentAssignment.RouteId != routeId)
            throw new InvalidOperationException("The bus is already assigned to another route");

        var assignment = new RouteBusAssignment
        {
            Id = Guid.NewGuid(),
            BusId = busId,
            RouteId = routeId,
            Status = Status.Active
        };

        var saved = await _assignmentRepository.SaveAsync(assignment, ct);

        await _scheduleProvisioner.SyncSchedulesAsync(
            saved.Id,
            route.StartTime,
            routeDays,
            ct);

        return _mapper.Map<BusAssignmentDto>(saved);
    }
}
