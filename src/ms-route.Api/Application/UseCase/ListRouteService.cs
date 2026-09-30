using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class ListRouteService : IListRouteUseCase
{
    private readonly IRouteRepository _repository;
    private readonly IMapper _mapper;

    public ListRouteService(IRouteRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RouteListDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var routes = await _repository.GetAllAsync(ct);

        return _mapper.Map<IEnumerable<RouteListDto>>(routes);
    }
}
