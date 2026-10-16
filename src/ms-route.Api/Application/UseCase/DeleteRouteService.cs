using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class DeleteRouteService : IDeleteRouteUseCase
{
    private readonly IRouteRepository _repository;
    private readonly IRouteExecutionRepository _executionRepository;

    public DeleteRouteService(
        IRouteRepository repository,
        IRouteExecutionRepository executionRepository)
    {
        _repository = repository;
        _executionRepository = executionRepository;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        // No se puede eliminar (desactivar) una ruta con un viaje en curso:
        // el conductor seguiria operando sobre una ruta que el admin cree dada de baja.
        if (await _executionRepository.HasActiveByRouteIdAsync(id, ct))
            throw new InvalidOperationException("The route has an active trip");

        await _repository.DeleteAsync(id, ct);
    }
}
