using ms_route.Api.Application.Dto;

namespace ms_route.Api.Application.Search.Route;

public interface IRouteSearchStrategy
{
    bool CanHandle(string search);

    IEnumerable<RouteListDto> Search(string search, IEnumerable<RouteListDto> routes);
}
