namespace ms_route.Api.Domain.Ports.In;

public interface IUnassignBusFromRouteUseCase
{
    Task ExecuteAsync(Guid routeId, CancellationToken ct = default);
}
