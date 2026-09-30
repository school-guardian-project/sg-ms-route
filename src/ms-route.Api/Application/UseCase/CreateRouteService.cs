using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.UseCase;

public class CreateRouteService : ICreateRouteUseCase
{
    private readonly IRouteRepository _repository;

    public CreateRouteService(IRouteRepository repository)
    {
        _repository = repository;
    }

    public async Task<RouteResponseDto> ExecuteAsync(RouteRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request.Name));

        var exists = await _repository.ExistsByNameAsync(request.Name, ct);
        if (exists)
            throw new InvalidOperationException("Route already exists");

        var route = new RouteModel
        {
            Id = Guid.NewGuid(),
            CampuseId = request.CampuseId,
            Name = request.Name.Trim(),
            TargetSector = request.TargetSector.Trim(),
            Status = RouteStatus.Active
        };

        var saved = await _repository.SaveAsync(route, ct);

        return new RouteResponseDto
        {
            Id = saved.Id,
            Name = saved.Name,
            CampuseId = saved.CampuseId.ToString(),
            TargetSector = saved.TargetSector,
            Status = saved.Status.ToString(),
            CreatedAt = saved.CreatedAt,
            UpdatedAt = saved.UpdatedAt
        };
    }
}
