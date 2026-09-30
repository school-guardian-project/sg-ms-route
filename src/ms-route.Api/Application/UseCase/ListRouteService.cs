using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using RouteModel = ms_route.Api.Domain.Model.Route;

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
            CampuseId = r.CampuseId.ToString(),
            TargetSector = r.TargetSector
        }).ToList();
    }
}
