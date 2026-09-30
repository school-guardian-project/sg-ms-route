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

    public async Task<IReadOnlyList<Stop>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _context.Stops
            .AsNoTracking()
            .ToListAsync(ct);

        return entities.Select(ToDomain).ToList();
    }

    public async Task<Stop> SaveAsync(Stop stop, CancellationToken ct = default)
    {
        var entity = new StopEntity
        {
            Id = stop.Id,
            CityId = stop.CityId,
            SchoolId = stop.SchoolId,
            Address = stop.Address,
            Longitude = stop.Longitude,
            Latitude = stop.Latitude,
            Status = stop.Status,
            CreatedAt = stop.CreatedAt
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

        entity.CityId = stop.CityId;
        entity.SchoolId = stop.SchoolId;
        entity.Address = stop.Address;
        entity.Longitude = stop.Longitude;
        entity.Latitude = stop.Latitude;
        entity.Status = stop.Status;
        entity.UpdatedAt = stop.UpdatedAt;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Stops
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (entity is null)
            throw new InvalidOperationException($"Stop not found: {id}");

        _context.Stops.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    private static Stop ToDomain(StopEntity entity) => new()
    {
        Id = entity.Id,
        CityId = entity.CityId,
        SchoolId = entity.SchoolId,
        Address = entity.Address,
        Longitude = entity.Longitude,
        Latitude = entity.Latitude,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
