using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.UseCase;

public class GetRouteService : IGetRouteUseCase
{
    private readonly IRouteRepository _repository;

    public GetRouteService(IRouteRepository repository)
    {
        _repository = repository;
    }

    public async Task<RouteResponseDto> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var route = await _repository.GetByIdAsync(id, ct);
        if (route is null)
            throw new InvalidOperationException($"Route not found: {id}");

        return new RouteResponseDto
        {
            Id = route.Id,
            Name = route.Name,
            CampuseId = route.CampuseId.ToString(),
            TargetSector = route.TargetSector,
            StartTime = route.StartTime,
            EndTime = route.EndTime,
            Status = route.Status.ToString(),
            CreatedAt = route.CreatedAt,
            UpdatedAt = route.UpdatedAt
        };
    }
}
