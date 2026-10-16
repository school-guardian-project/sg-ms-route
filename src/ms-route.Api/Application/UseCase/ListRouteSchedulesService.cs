using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class ListRouteSchedulesService : IListRouteSchedulesUseCase
{
    private readonly IRouteRepository _routeRepository;
    private readonly IRouteScheduleRepository _scheduleRepository;

    public ListRouteSchedulesService(
        IRouteRepository routeRepository,
        IRouteScheduleRepository scheduleRepository)
    {
        _routeRepository = routeRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<IReadOnlyList<ScheduleDetailDto>> ExecuteAsync(
        Guid routeId,
        CancellationToken ct = default)
    {
        var route = await _routeRepository.GetByIdAsync(routeId, ct);
        if (route is null)
            throw new InvalidOperationException($"Route not found: {routeId}");

        var schedules = await _scheduleRepository.GetByRouteIdAsync(routeId, ct);

        return schedules
            .Select(s => new ScheduleDetailDto
            {
                Id = s.Id,
                DayOfWeek = s.DayOfWeek.ToString(),
                Direction = s.Direction,
                StartTime = s.StartTime
            })
            .ToList();
    }
}
