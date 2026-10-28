using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Domain.Service;

namespace ms_route.Api.Application.UseCase;

public class CreateRouteScheduleService : ICreateRouteScheduleUseCase
{
    private readonly IRouteScheduleProvisioner _scheduleProvisioner;
    private readonly IRouteBusAssignmentRepository _assignmentRepository;
    private readonly IRouteRepository _routeRepository;

    public CreateRouteScheduleService(
        IRouteScheduleProvisioner scheduleProvisioner,
        IRouteBusAssignmentRepository assignmentRepository,
        IRouteRepository routeRepository)
    {
        _scheduleProvisioner = scheduleProvisioner;
        _assignmentRepository = assignmentRepository;
        _routeRepository = routeRepository;
    }

    public async Task<List<ScheduleDetailDto>> ExecuteAsync(
        Guid routeId,
        RouteRequestDto request,
        CancellationToken ct = default)
    {
        var route = await _routeRepository.GetByIdAsync(routeId, ct);
        if (route is null)
            throw new InvalidOperationException("Route not found");

        if (request.DaysOfWeek is null || request.DaysOfWeek.Count == 0)
            throw new ArgumentException("DaysOfWeek is required.", nameof(request.DaysOfWeek));

        // Se validan todos los dias antes de escribir: un dia invalido no deja
        // horarios a medio crear.
        var days = RouteScheduleProvisioner.ParseConfiguredDays(request.DaysOfWeek);

        var assignment = await _assignmentRepository.GetActiveByRouteIdAsync(routeId, ct);
        if (assignment is null)
            throw new InvalidOperationException("The route has no bus assigned");

        var created = await _scheduleProvisioner.EnsureSchedulesAsync(assignment.Id, route.StartTime, days, ct);

        return created
            .Select(saved => new ScheduleDetailDto
            {
                Id = saved.Id,
                DayOfWeek = saved.DayOfWeek.ToString(),
                Direction = saved.Direction,
                StartTime = saved.StartTime
            })
            .ToList();
    }

}
