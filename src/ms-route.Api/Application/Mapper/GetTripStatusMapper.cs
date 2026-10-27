using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

public sealed class GetTripStatusMapper : IGetTripStatusMapper
{
    public GetTripStatusResponseDto ToResponse(TripStatusDto? source)
    {
        if (source is null)
        {
            return new GetTripStatusResponseDto { Found = false };
        }

        return new GetTripStatusResponseDto
        {
            Found = true,
            TripId = source.TripId == Guid.Empty ? null : source.TripId,
            RouteId = source.RouteId,
            RouteName = source.RouteName,
            Direction = source.Direction,
            Status = source.Status,
            CanStart = source.CanStart,
            CanEnd = source.CanEnd,
            Message = source.Message,
            ScheduleStartTime = source.ScheduleStartTime,
            NextScheduleTime = source.NextScheduleTime,
            EndConditionDetail = ToEndConditionDetail(source.EndConditionDetail)
        };
    }

    public TripEndConditionDetailDto? ToEndConditionDetail(TripEndConditionDetail? source)
    {
        if (source is null) return null;

        return new TripEndConditionDetailDto
        {
            CanEnd = source.CanEnd,
            TotalAssignedStudents = source.TotalAssignedStudents,
            StudentsAlightedAtFinalStop = source.StudentsAlightedAtFinalStop,
            IsFinalStopReached = source.IsFinalStopReached,
            FinalStopName = source.FinalStopName,
            Message = source.Message
        };
    }
}