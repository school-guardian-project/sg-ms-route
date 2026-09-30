using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class ListRouteService : IListRouteUseCase
{
    private readonly IRouteRepository _repository;

    public ListRouteService(IRouteRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RouteListDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var routes = await _repository.GetAllAsync(ct);

        return routes.Select(r => new RouteListDto
        {
            Id = r.Id,
            Name = r.Name,
            TargetSector = r.TargetSector,
            Status = r.Status.ToString()
        }).ToList();
    }
}
