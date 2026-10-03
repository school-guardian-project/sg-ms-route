using ms_route.Api.Application.Dto;

namespace ms_route.Api.Application.Search.Stop;

public interface IStopSearchStrategy
{
    bool CanHandle(string search);

    IEnumerable<StopListDto> Search(string search, IEnumerable<StopListDto> stops);
}
