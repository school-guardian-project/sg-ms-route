using AutoMapper;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class UpdateStopService : IUpdateStopUseCase
{
    private readonly IStopRepository _repository;
    private readonly IRouteRepository _routeRepository;
    private readonly IRouteStopRepository _routeStopRepository;
    private readonly IMapper _mapper;

    public UpdateStopService(
        IStopRepository repository,
        IRouteRepository routeRepository,
        IRouteStopRepository routeStopRepository,
        IMapper mapper)
    {
        _repository = repository;
        _routeRepository = routeRepository;
        _routeStopRepository = routeStopRepository;
        _mapper = mapper;
    }

    public async Task ExecuteAsync(Guid id, StopRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request.Name));
        if (string.IsNullOrWhiteSpace(request.Address))
            throw new ArgumentException("Address is required.", nameof(request.Address));

        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing is null)
            throw new InvalidOperationException($"Stop not found: {id}");

        if (request.RouteId is { } routeId && routeId != Guid.Empty)
        {
            var route = await _routeRepository.GetByIdAsync(routeId, ct);
            if (route is null || route.Status != Domain.Model.Status.Active)
                throw new InvalidOperationException($"Route not found: {routeId}");
        }

        _mapper.Map(request, existing);

        await _repository.UpdateAsync(existing, ct);

        if (request.RouteId is { } newRouteId && newRouteId != Guid.Empty)
            await _routeStopRepository.MoveStopToRouteAsync(id, request.PreviousRouteId, newRouteId, ct);
    }
}
