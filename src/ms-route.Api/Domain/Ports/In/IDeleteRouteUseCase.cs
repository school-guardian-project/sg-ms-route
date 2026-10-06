namespace ms_route.Api.Domain.Ports.In;

public interface IDeleteRouteUseCase
{
    Task ExecuteAsync(Guid id, CancellationToken ct = default);
}
