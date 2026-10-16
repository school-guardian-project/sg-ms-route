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

    private readonly IRouteScheduleRepository _scheduleRepository;
    private readonly IRouteBusAssignmentRepository _assignmentRepository;

    public CreateRouteScheduleService(
        IRouteScheduleRepository scheduleRepository,
        IRouteBusAssignmentRepository assignmentRepository)
    {
        _scheduleRepository = scheduleRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<ScheduleDetailDto> ExecuteAsync(
        Guid routeId,
        RouteScheduleRequestDto request,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<DayOfWeek>(request.DayOfWeek, ignoreCase: true, out var dayOfWeek)
            || !AllowedDays.Contains(dayOfWeek))
        {
            throw new ArgumentException(
                "DayOfWeek must be one of Monday, Tuesday, Wednesday, Thursday or Friday.",
                nameof(request.DayOfWeek));
        }

        var direction = request.Direction?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(direction))
            throw new ArgumentException("Direction is required.", nameof(request.Direction));

        if (direction.Length > DirectionMaxLength)
        {
            throw new ArgumentException(
                $"Direction must be at most {DirectionMaxLength} characters.",
                nameof(request.Direction));
        }

        // El horario cuelga de la asignacion bus-ruta: sin bus asignado no hay
        // donde registrarlo.
        var assignment = await _assignmentRepository.GetActiveByRouteIdAsync(routeId, ct);
        if (assignment is null)
            throw new InvalidOperationException("The route has no bus assigned");

        if (await _scheduleRepository.ExistsActiveAsync(assignment.Id, dayOfWeek, direction, ct))
            throw new InvalidOperationException("A schedule already exists for that day and direction");

        var schedule = new RouteSchedule
        {
            Id = Guid.NewGuid(),
            RouteBusAssignmentsId = assignment.Id,
            DayOfWeek = dayOfWeek,
            Direction = direction,
            StartTime = request.StartTime,
            Status = Status.Active
        };

        var saved = await _scheduleRepository.SaveAsync(schedule, ct);

        return new ScheduleDetailDto
        {
            Id = saved.Id,
            DayOfWeek = saved.DayOfWeek.ToString(),
            Direction = saved.Direction,
            StartTime = saved.StartTime
        };
    }
}
