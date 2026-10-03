using System.Globalization;
using ms_route.Api.Application.Dto;

namespace ms_route.Api.Application.Search.Stop;

public class NameSearchStrategy : IStopSearchStrategy
{
    public bool CanHandle(string search)
    {
        return true;
    }

    public IEnumerable<StopListDto> Search(string search, IEnumerable<StopListDto> stops)
    {
        return stops.Where(stop =>
            stop.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || stop.Address.Contains(search, StringComparison.OrdinalIgnoreCase)
            || stop.Latitude.ToString(CultureInfo.InvariantCulture).Contains(search, StringComparison.OrdinalIgnoreCase)
            || stop.Longitude.ToString(CultureInfo.InvariantCulture).Contains(search, StringComparison.OrdinalIgnoreCase));
    }
}
