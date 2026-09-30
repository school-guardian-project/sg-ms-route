using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Infrastructure.Persistence.Context;
using ms_route.Api.Infrastructure.Persistence.Entity;
using RouteModel = ms_route.Api.Domain.Model.Route;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.Repository;

public class RouteRepositoryImpl : IRouteRepository
{
    private readonly RouteContext _context;

    public RouteRepositoryImpl(RouteContext context)
    {
        _context = context;
    }

    public async Task<RouteModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Routes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task<IReadOnlyList<RouteModel>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _context.Routes
            .AsNoTracking()
            .ToListAsync(ct);

        return entities.Select(ToDomain).ToList();
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Routes
            .AnyAsync(r => r.Name == name, ct);
    }

    public async Task<RouteModel> SaveAsync(RouteModel route, CancellationToken ct = default)
    {
        var entity = new RouteEntity
        {
            Id = route.Id,
            CampuseId = route.CampuseId,
            Name = route.Name,
            TargetSector = route.TargetSector,
            Status = route.Status,
            CreatedAt = route.CreatedAt
        };

        await _context.Routes.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return ToDomain(entity);
    }

    public async Task UpdateAsync(RouteModel route, CancellationToken ct = default)
    {
        var entity = await _context.Routes
            .FirstOrDefaultAsync(r => r.Id == route.Id, ct);

        if (entity is null)
            throw new InvalidOperationException($"Route not found: {route.Id}");

        entity.Name = route.Name;
        entity.TargetSector = route.TargetSector;
        entity.CampuseId = route.CampuseId;
        entity.Status = route.Status;
        entity.UpdatedAt = route.UpdatedAt;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Routes
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (entity is null)
            throw new InvalidOperationException($"Route not found: {id}");

        _context.Routes.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    private static RouteModel ToDomain(RouteEntity entity) => new()
    {
        Id = entity.Id,
        CampuseId = entity.CampuseId,
        Name = entity.Name,
        TargetSector = entity.TargetSector,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
