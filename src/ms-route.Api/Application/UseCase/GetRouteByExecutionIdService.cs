using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Application.UseCase;

public class GetRouteByExecutionIdService(
    IRouteExecutionRepository executionRepository,
    IRouteRepository routeRepository) : IGetRouteByExecutionIdUseCase
{
    public async Task<RouteModel?> ExecuteAsync(Guid executionId, CancellationToken ct = default)
    {
        var execution = await executionRepository.GetByIdAsync(executionId, ct);
        return execution is null ? null : await routeRepository.GetByIdAsync(execution.RouteId, ct);
    }
}
