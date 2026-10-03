using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class InMemoryRouteExecutionRepository : IRouteExecutionRepository
{
    public readonly List<RouteExecution> Executions = new();

    public Task<RouteExecution?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Executions.FirstOrDefault(e => e.Id == id));

    public Task<RouteExecution?> GetActiveByDriverAsync(Guid driverId, CancellationToken ct = default)
        => Task.FromResult(Executions.FirstOrDefault(e => e.DriverId == driverId && e.Status == Status.Active));

    public Task<RouteExecution> SaveAsync(RouteExecution execution, CancellationToken ct = default)
    {
        Executions.Add(execution);
        return Task.FromResult(execution);
    }

    public Task UpdateAsync(RouteExecution execution, CancellationToken ct = default)
    {
        var index = Executions.FindIndex(e => e.Id == execution.Id);
        if (index >= 0)
            Executions[index] = execution;

        return Task.CompletedTask;
    }
}
