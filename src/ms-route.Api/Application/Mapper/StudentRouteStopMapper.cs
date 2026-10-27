using ms_route.Api.Application.Dto.Response;
using ms_route.Api.Domain.Model;

namespace ms_route.Api.Application.Mapper;

/// <summary>
/// Maps StudentRouteStop domain entities to Response DTOs.
/// This mapper is stateless and can be registered as a singleton.
/// </summary>
public interface IStudentRouteStopMapper
{
    StudentRouteStopResponse ToResponse(
        RouteStudentAssignment assignment,
        RouteStop routeStop,
        Stop stop);
}

public sealed class StudentRouteStopMapper : IStudentRouteStopMapper
{
    public StudentRouteStopResponse ToResponse(
        RouteStudentAssignment assignment,
        RouteStop routeStop,
        Stop stop)
    {
        return new StudentRouteStopResponse(
            routeStop.RouteId,
            assignment.ProfileId,
            routeStop.Id,
            stop.Id,
            stop.Name,
            stop.Address,
            stop.Latitude,
            stop.Longitude);
    }
}