using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.UseCase;

public class UpdateRouteService : IUpdateRouteUseCase
{
    private readonly IRouteRepository _repository;

    public UpdateRouteService(IRouteRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Guid id, RouteRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request.Name));

        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing is null)
            throw new InvalidOperationException($"Route not found: {id}");

        existing.Name = request.Name.Trim();
        existing.TargetSector = request.TargetSector.Trim();
        existing.CampuseId = request.CampuseId;
        existing.StartTime = request.StartTime;
        existing.EndTime = request.EndTime;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existing, ct);
    }
}
