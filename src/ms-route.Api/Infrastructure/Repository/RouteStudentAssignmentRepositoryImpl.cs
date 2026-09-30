using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Infrastructure.Persistence.Context;
using ms_route.Api.Infrastructure.Persistence.Entity;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.Repository;

public class RouteStudentAssignmentRepositoryImpl : IRouteStudentAssignmentRepository
{
    private readonly RouteContext _context;

    public RouteStudentAssignmentRepositoryImpl(RouteContext context)
    {
        _context = context;
    }

    public async Task<RouteStudentAssignment> SaveAsync(RouteStudentAssignment assignment, CancellationToken ct = default)
    {
        var entity = new RouteStudentAssignmentEntity
        {
            Id = assignment.Id,
            ProfileId = assignment.ProfileId,
            RouteStopId = assignment.RouteStopId,
            Status = assignment.Status
        };

        await _context.RouteStudentAssignments.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return ToDomain(entity);
    }

    private static RouteStudentAssignment ToDomain(RouteStudentAssignmentEntity entity) => new()
    {
        Id = entity.Id,
        ProfileId = entity.ProfileId,
        RouteStopId = entity.RouteStopId,
        Status = entity.Status
    };
}
