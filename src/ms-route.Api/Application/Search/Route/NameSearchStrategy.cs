using ms_route.Api.Application.Dto;

namespace ms_route.Api.Application.Search.Route;

public class NameSearchStrategy : IRouteSearchStrategy
{
    public bool CanHandle(string search)
    {
        return true;
    }

    public IEnumerable<RouteListDto> Search(string search, IEnumerable<RouteListDto> routes)
    {
        return routes.Where(route =>
            route.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || route.TargetSector.Contains(search, StringComparison.OrdinalIgnoreCase));
    }
}
