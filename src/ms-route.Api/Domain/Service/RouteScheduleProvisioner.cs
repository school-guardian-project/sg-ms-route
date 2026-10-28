using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Domain.Service;

public class RouteScheduleProvisioner : IRouteScheduleProvisioner
{
    /// <summary>Dias de operacion del transporte escolar.</summary>
    public static readonly IReadOnlyList<DayOfWeek> WorkingDays =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    ];

    /// <summary>Cada dia de operacion tiene recorrido de ida y de vuelta.</summary>
    public static readonly IReadOnlyList<string> Directions = ["Ida", "Vuelta"];

    private readonly IRouteScheduleRepository _scheduleRepository;

    public RouteScheduleProvisioner(IRouteScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public static IReadOnlyList<DayOfWeek> ParseConfiguredDays(IEnumerable<string>? configuredDays)
    {
        var values = configuredDays?.ToList() ?? [];
        if (values.Count == 0)
            return WorkingDays;

        var days = new List<DayOfWeek>();
        foreach (var value in values)
        {
            if (!Enum.TryParse<DayOfWeek>(value, ignoreCase: true, out var day)
                || !WorkingDays.Contains(day))
            {
                throw new ArgumentException(
                    "DayOfWeek must be one of Monday, Tuesday, Wednesday, Thursday or Friday.",
                    nameof(configuredDays));
            }

            if (!days.Contains(day))
                days.Add(day);
        }

        return days;
    }

    public async Task<IReadOnlyList<RouteSchedule>> EnsureSchedulesAsync(
        Guid routeBusAssignmentId,
        TimeOnly startTime,
        IEnumerable<DayOfWeek> days,
        CancellationToken ct = default)
    {
        var created = new List<RouteSchedule>();

        foreach (var day in days.Distinct())
        {
            if (!WorkingDays.Contains(day))
                throw new ArgumentException(
                    "DayOfWeek must be one of Monday, Tuesday, Wednesday, Thursday or Friday.",
                    nameof(days));

            foreach (var direction in Directions)
            {
                if (await _scheduleRepository.ExistsActiveAsync(routeBusAssignmentId, day, direction, ct))
                    continue;

                created.Add(await _scheduleRepository.SaveAsync(new RouteSchedule
                {
                    Id = Guid.NewGuid(),
                    RouteBusAssignmentsId = routeBusAssignmentId,
                    DayOfWeek = day,
                    Direction = direction,
                    StartTime = startTime,
                    Status = Status.Active
                }, ct));
            }
        }

        return created;
    }

    public Task SyncStartTimeAsync(
        Guid routeBusAssignmentId,
        TimeOnly startTime,
        CancellationToken ct = default)
        => _scheduleRepository.UpdateStartTimeAsync(routeBusAssignmentId, startTime, ct);

    public Task SyncSchedulesAsync(
        Guid routeBusAssignmentId,
        TimeOnly startTime,
        IEnumerable<DayOfWeek> days,
        CancellationToken ct = default)
    {
        var validatedDays = days.Distinct().ToList();
        if (validatedDays.Any(day => !WorkingDays.Contains(day)))
            throw new ArgumentException(
                "DayOfWeek must be one of Monday, Tuesday, Wednesday, Thursday or Friday.",
                nameof(days));

        return _scheduleRepository.SynchronizeActiveDaysAsync(
            routeBusAssignmentId, startTime, validatedDays, ct);
    }
}
