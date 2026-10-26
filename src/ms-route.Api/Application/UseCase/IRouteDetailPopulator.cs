using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public interface IRouteStopFilter
{
    IEnumerable<RouteStop> FilterStops(
        IEnumerable<RouteStop> routeStops,
        Dictionary<Guid, Stop> stopById,
        RouteStudentAssignment? assignment,
        bool isParent,
        Guid? studentProfileId);
}

public interface IRouteDetailPopulator
{
    Task PopulateAsync(RouteDetailDto detail, Guid routeId, CancellationToken ct, bool isParent = false, Guid? studentProfileId = null, RouteStudentAssignment? assignment = null);
}