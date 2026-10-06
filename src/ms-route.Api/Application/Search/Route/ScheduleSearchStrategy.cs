using System.Text.RegularExpressions;
using ms_route.Api.Application.Dto;

namespace ms_route.Api.Application.Search.Route;

public class ScheduleSearchStrategy : IRouteSearchStrategy
{
    private static readonly Regex TimeShape = new(@"^(\d|[01]\d|2[0-3]):[0-5]\d$", RegexOptions.Compiled);

    public bool CanHandle(string search)
    {
        return TimeShape.IsMatch(search);
    }

    public IEnumerable<RouteListDto> Search(string search, IEnumerable<RouteListDto> routes)
    {
        return routes.Where(route => Matches(route.StartTime, search) || Matches(route.EndTime, search));
    }

    private static bool Matches(TimeOnly schedule, string search)
    {
        var shortForm = $"{schedule.Hour}:{schedule.Minute:00}";
        var paddedForm = $"{schedule.Hour:00}:{schedule.Minute:00}";

        return shortForm.Contains(search, StringComparison.OrdinalIgnoreCase)
            || paddedForm.Contains(search, StringComparison.OrdinalIgnoreCase);
    }
}
