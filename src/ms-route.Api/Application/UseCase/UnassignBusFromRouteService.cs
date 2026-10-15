using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Application.UseCase;

public class UnassignBusFromRouteService : IUnassignBusFromRouteUseCase
{
    private readonly IRouteBusAssignmentRepository _assignmentRepository;

    public UnassignBusFromRouteService(IRouteBusAssignmentRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    public async Task ExecuteAsync(Guid routeId, CancellationToken ct = default)
    {
        var released = await _assignmentRepository.UnassignAsync(routeId, ct);
        if (!released)
            throw new InvalidOperationException("The route has no bus assigned");
    }
}
