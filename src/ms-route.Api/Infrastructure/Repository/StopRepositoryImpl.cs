using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Infrastructure.Persistence.Context;
using ms_route.Api.Infrastructure.Persistence.Entity;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.Repository;

public class StopRepositoryImpl : IStopRepository
{
    private readonly RouteContext _context;

    public StopRepositoryImpl(RouteContext context)
    {
        _context = context;
    }

    public async Task<Stop?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Stops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task<IReadOnlyList<Stop>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return new List<Stop>();

        var entities = await _context.Stops
            .AsNoTracking()
            .Where(s => idList.Contains(s.Id))
            .ToListAsync(ct);

        return entities.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Stop>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _context.Stops
            .AsNoTracking()
            .Where(s => s.Status == Status.Active)
            .ToListAsync(ct);

        return entities.Select(ToDomain).ToList();
    }

    public async Task<Stop> SaveAsync(Stop stop, CancellationToken ct = default)
    {
        var entity = new StopEntity
        {
            Id = stop.Id,
            Name = stop.Name,
            CityId = stop.CityId,
            SchoolId = stop.SchoolId,
            Address = stop.Address,
            Longitude = stop.Longitude,
            Latitude = stop.Latitude,
            Status = stop.Status
        };

        await _context.Stops.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return ToDomain(entity);
    }

    public async Task UpdateAsync(Stop stop, CancellationToken ct = default)
    {
        var entity = await _context.Stops
            .FirstOrDefaultAsync(s => s.Id == stop.Id, ct);

        if (entity is null)
            throw new InvalidOperationException($"Stop not found: {stop.Id}");

        entity.Name = stop.Name;
        entity.CityId = stop.CityId;
        entity.SchoolId = stop.SchoolId;
        entity.Address = stop.Address;
        entity.Longitude = stop.Longitude;
        entity.Latitude = stop.Latitude;
        entity.Status = stop.Status;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Stops
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (entity is null)
            throw new InvalidOperationException($"Stop not found: {id}");

        // Borrado lógico: RouteStop la referencia sin cascada.
        entity.Status = Status.Inactive;
        await _context.SaveChangesAsync(ct);
    }

    private static Stop ToDomain(StopEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        CityId = entity.CityId,
        SchoolId = entity.SchoolId,
        Address = entity.Address,
        Longitude = entity.Longitude,
        Latitude = entity.Latitude,
        Status = entity.Status
    };
}
