using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

public interface IGetTripStatusMapper
{
    GetTripStatusResponseDto ToResponse(TripStatusDto? source);
    TripEndConditionDetailDto? ToEndConditionDetail(TripEndConditionDetail? source);
}