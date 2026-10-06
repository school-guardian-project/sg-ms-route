namespace ms_route.Api.Domain.Ports.In;

public interface IDeleteStopUseCase
{
    Task ExecuteAsync(Guid id, CancellationToken ct = default);
}
