using ms_route.Api.Application.Dto.Response;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps Route domain entities to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface IRouteMapper
{
    RouteListResponse ToListResponse(RouteModel route);
    RouteResponse ToResponse(RouteModel route, Guid? busId, List<StopDetailResponse> stops);
    RouteDetailResponse ToDetailResponse(
        RouteModel route,
        List<StopDetailResponse> stops,
        List<ScheduleDetailResponse> schedules,
        RouteBusResponse? bus = null);
}

public sealed class RouteMapper : IRouteMapper
{
    public RouteListResponse ToListResponse(RouteModel route)
    {
        return new RouteListResponse(
            route.Id,
            route.Name,
            route.CampuseId.ToString(),
            route.TargetSector,
            route.StartTime,
            route.EndTime,
            route.DaysOfWeek,
            0); // StopsCount needs to be calculated externally
    }

    public RouteResponse ToResponse(RouteModel route, Guid? busId, List<StopDetailResponse> stops)
    {
        return new RouteResponse(
            route.Id,
            route.Name,
            route.CampuseId.ToString(),
            route.TargetSector,
            route.StartTime,
            route.EndTime,
            route.DaysOfWeek,
            route.Status.ToString(),
            busId,
            stops);
    }

    public RouteDetailResponse ToDetailResponse(
        RouteModel route,
        List<StopDetailResponse> stops,
        List<ScheduleDetailResponse> schedules,
        RouteBusResponse? bus = null)
    {
        return new RouteDetailResponse(
            route.Id,
            route.Name,
            route.CampuseId.ToString(),
            route.TargetSector,
            route.StartTime,
            route.EndTime,
            route.Status.ToString(),
            route.DaysOfWeek,
            stops,
            schedules,
            bus);
    }
}