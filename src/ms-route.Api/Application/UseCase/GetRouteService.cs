using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class GetRouteService : IGetRouteUseCase
{
    private readonly IRouteRepository _repository;
    private readonly IMapper _mapper;

    public GetRouteService(IRouteRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<RouteResponseDto> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var route = await _repository.GetByIdAsync(id, ct);
        if (route is null)
            throw new InvalidOperationException($"Route not found: {id}");

        return _mapper.Map<RouteResponseDto>(route);
    }
}
