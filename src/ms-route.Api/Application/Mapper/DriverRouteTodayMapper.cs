using ms_route.Api.Application.Dto.Response;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps DriverRouteToday domain entities to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface IDriverRouteTodayMapper
{
    DriverRouteTodayResponse ToResponse(
        string status,
        RouteDetailResponse? route,
        DateTimeOffset? nextOccurrence);
}

public sealed class DriverRouteTodayMapper : IDriverRouteTodayMapper
{
    public DriverRouteTodayResponse ToResponse(
        string status,
        RouteDetailResponse? route,
        DateTimeOffset? nextOccurrence)
    {
        return new DriverRouteTodayResponse(status, route, nextOccurrence);
    }
}