using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.UseCase;

public class CreateRouteService : ICreateRouteUseCase
{
    private readonly IRouteRepository _repository;
    private readonly IMapper _mapper;

    public CreateRouteService(IRouteRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<RouteResponseDto> ExecuteAsync(RouteRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request.Name));

        var exists = await _repository.ExistsByNameAsync(request.Name, ct);
        if (exists)
            throw new InvalidOperationException("Route already exists");

        var route = _mapper.Map<RouteModel>(request);
        route.Id = Guid.NewGuid();
        route.Status = Status.Active;

        var saved = await _repository.SaveAsync(route, ct);

        return _mapper.Map<RouteResponseDto>(saved);
    }
}
