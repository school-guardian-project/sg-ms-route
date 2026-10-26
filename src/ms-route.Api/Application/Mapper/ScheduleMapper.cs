using ms_route.Api.Application.Dto.Response;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps Schedule domain entities to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface IScheduleMapper
{
    ScheduleDetailResponse ToDetailResponse(RouteSchedule schedule);
}

public sealed class ScheduleMapper : IScheduleMapper
{
    public ScheduleDetailResponse ToDetailResponse(RouteSchedule schedule)
    {
        return new ScheduleDetailResponse(
            schedule.Id,
            schedule.DayOfWeek,
            schedule.Direction,
            schedule.StartTime);
    }
}