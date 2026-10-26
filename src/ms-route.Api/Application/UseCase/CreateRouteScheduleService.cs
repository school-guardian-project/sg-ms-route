using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class CreateRouteScheduleService : ICreateRouteScheduleUseCase
{
    // El CHECK de la tabla y la ventana activa del conductor solo contemplan
    // dias laborables, asi que se rechazan sabado y domingo.
    private static readonly DayOfWeek[] AllowedDays =
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    };

    private const int DirectionMaxLength = 10;
    private static readonly string[] Directions = { "Ida", "Vuelta" };

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

        var daysOfWeek = request.DaysOfWeek ?? new List<string>();
        if (daysOfWeek.Count == 0)
            throw new ArgumentException("At least one day of week is required.", nameof(request.DaysOfWeek));

        // Validate days
        var parsedDays = new List<DayOfWeek>();
        foreach (var dayStr in daysOfWeek)
        {
            if (!Enum.TryParse<DayOfWeek>(dayStr, ignoreCase: true, out var dayOfWeek)
                || !AllowedDays.Contains(dayOfWeek))
            {
                throw new ArgumentException(
                    "DayOfWeek must be one of Monday, Tuesday, Wednesday, Thursday or Friday.",
                    nameof(request.DaysOfWeek));
            }
            parsedDays.Add(dayOfWeek);
        }

        // El horario cuelga de la asignacion bus-ruta: sin bus asignado no hay
        // donde registrarlo.
        var assignment = await _assignmentRepository.GetActiveByRouteIdAsync(routeId, ct);
        if (assignment is null)
            throw new InvalidOperationException("The route has no bus assigned");

        var result = new List<ScheduleDetailDto>();

        foreach (var dayOfWeek in parsedDays)
        {
            foreach (var direction in Directions)
            {
                if (await _scheduleRepository.ExistsActiveAsync(assignment.Id, dayOfWeek, direction, ct))
                    throw new InvalidOperationException($"A schedule already exists for {dayOfWeek} {direction}");

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

                result.Add(new ScheduleDetailDto
                {
                    Id = saved.Id,
                    DayOfWeek = saved.DayOfWeek.ToString(),
                    Direction = saved.Direction,
                    StartTime = saved.StartTime
                });
            }
        }

        return result;
    }
}
