using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Infrastructure.Persistence.Context;
using ms_route.Api.Infrastructure.Persistence.Entity;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.Repository;

public class RouteBusAssignmentRepositoryImpl : IRouteBusAssignmentRepository
{
    private readonly RouteContext _context;

    public RouteBusAssignmentRepositoryImpl(RouteContext context)
    {
        _context = context;
    }

    public async Task<RouteBusAssignment> SaveAsync(RouteBusAssignment assignment, CancellationToken ct = default)
    {
        var entity = new RouteBusAssignmentEntity
        {
            Id = assignment.Id,
            BusId = assignment.BusId,
            RouteId = assignment.RouteId,
            Status = assignment.Status
        };

        await _context.RouteBusAssignments.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return ToDomain(entity);
    }

    public async Task<bool> RouteHasBusAsync(Guid routeId, CancellationToken ct = default)
    {
        return await _context.RouteBusAssignments
            .AnyAsync(a => a.RouteId == routeId && a.Status == Status.Active, ct);
    }

    private static RouteBusAssignment ToDomain(RouteBusAssignmentEntity entity) => new()
    {
        Id = entity.Id,
        BusId = entity.BusId,
        RouteId = entity.RouteId,
        Status = entity.Status
    };
}
