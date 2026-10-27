using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Api.Domain.Ports.In;

public interface IGetRouteByExecutionIdUseCase
{
    Task<RouteModel?> ExecuteAsync(Guid executionId, CancellationToken ct = default);
}
