using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Infrastructure.Persistence.Context;
using ms_route.Api.Infrastructure.Persistence.Entity;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.Repository;

public class RouteExecutionRepositoryImpl : IRouteExecutionRepository
{
    private readonly RouteContext _context;

    public RouteExecutionRepositoryImpl(RouteContext context)
    {
        _context = context;
    }

    public async Task<RouteExecution?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.RouteExecutions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task<RouteExecution> SaveAsync(RouteExecution execution, CancellationToken ct = default)
    {
        var entity = new RouteExecutionEntity
        {
            Id = execution.Id,
            BusId = execution.BusId,
            DriverId = execution.DriverId,
            StartDateTime = execution.StartDateTime,
            EndDateTime = execution.EndDateTime,
            Status = execution.Status
        };

        await _context.RouteExecutions.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return ToDomain(entity);
    }

    public async Task UpdateAsync(RouteExecution execution, CancellationToken ct = default)
    {
        var entity = await _context.RouteExecutions
            .FirstOrDefaultAsync(e => e.Id == execution.Id, ct);

        if (entity is null)
            throw new InvalidOperationException($"RouteExecution not found: {execution.Id}");

        entity.EndDateTime = execution.EndDateTime;
        entity.Status = execution.Status;

        await _context.SaveChangesAsync(ct);
    }

    public async Task<RouteExecution?> GetActiveByDriverAsync(Guid driverId, CancellationToken ct = default)
    {
        var entity = await _context.RouteExecutions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.DriverId == driverId && e.Status == Status.Active, ct);

        return entity is null ? null : ToDomain(entity);
    }

    private static RouteExecution ToDomain(RouteExecutionEntity entity) => new()
    {
        Id = entity.Id,
        BusId = entity.BusId,
        DriverId = entity.DriverId,
        StartDateTime = entity.StartDateTime,
        EndDateTime = entity.EndDateTime,
        Status = entity.Status
    };
}
