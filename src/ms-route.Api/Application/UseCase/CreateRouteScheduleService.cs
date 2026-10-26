using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class CreateRouteScheduleService : ICreateRouteScheduleUseCase
{
    private static readonly DayOfWeek[] AllowedDays =
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    };

    private const int DirectionMaxLength = 10;

    private readonly IRouteScheduleRepository _scheduleRepository;
    private readonly IRouteBusAssignmentRepository _assignmentRepository;
    private readonly IRouteRepository _routeRepository;

    public CreateRouteScheduleService(
        IRouteScheduleRepository scheduleRepository,
        IRouteBusAssignmentRepository assignmentRepository,
        IRouteRepository routeRepository)
    {
        _scheduleRepository = scheduleRepository;
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

        var assignment = await _assignmentRepository.GetActiveByRouteIdAsync(routeId, ct);
        if (assignment is null)
            throw new InvalidOperationException("The route has no bus assigned");

        var results = new List<ScheduleDetailDto>();

        foreach (var dayStr in request.DaysOfWeek)
        {
            if (!Enum.TryParse<DayOfWeek>(dayStr, ignoreCase: true, out var dayOfWeek)
                || !AllowedDays.Contains(dayOfWeek))
            {
                throw new ArgumentException(
                    "DayOfWeek must be one of Monday, Tuesday, Wednesday, Thursday or Friday.",
                    nameof(request.DaysOfWeek));
            }

            foreach (var direction in new[] { "Ida", "Vuelta" })
            {
                if (await _scheduleRepository.ExistsActiveAsync(assignment.Id, dayOfWeek, direction, ct))
                    continue;

                var schedule = new RouteSchedule
                {
                    Id = Guid.NewGuid(),
                    RouteBusAssignmentsId = assignment.Id,
                    DayOfWeek = dayOfWeek,
                    Direction = direction,
                    StartTime = route.StartTime,
                    Status = Status.Active
                };

                var saved = await _scheduleRepository.SaveAsync(schedule, ct);

                results.Add(new ScheduleDetailDto
                {
                    Id = saved.Id,
                    DayOfWeek = saved.DayOfWeek.ToString(),
                    Direction = saved.Direction,
                    StartTime = saved.StartTime
                });
            }
        }

        return results;
    }
}
