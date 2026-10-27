using ms_route.Api.Application.Dto.Response;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps Stop domain entities to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface IStopMapper
{
    StopResponse ToResponse(Stop stop);
    StopListResponse ToListResponse(Stop stop, Guid? routeId, string routeName, List<string> routeNames);
    StopDetailResponse ToDetailResponse(RouteStop routeStop, Stop stop);
}

public sealed class StopMapper : IStopMapper
{
    public StopResponse ToResponse(Stop stop)
    {
        return new StopResponse(
            stop.Id,
            stop.Name,
            stop.Address,
            stop.Latitude,
            stop.Longitude,
            stop.CityId.ToString(),
            stop.SchoolId.ToString(),
            stop.Status.ToString());
    }

    public StopListResponse ToListResponse(Stop stop, Guid? routeId, string routeName, List<string> routeNames)
    {
        return new StopListResponse(
            stop.Id,
            stop.Name,
            stop.Address,
            stop.Latitude,
            stop.Longitude,
            stop.CityId,
            stop.SchoolId,
            routeId,
            routeName,
            routeNames);
    }

    public StopDetailResponse ToDetailResponse(RouteStop routeStop, Stop stop)
    {
        return new StopDetailResponse(
            stop.Id,
            stop.Name,
            stop.Address,
            stop.Latitude,
            stop.Longitude,
            routeStop.OrderSequence);
    }
}