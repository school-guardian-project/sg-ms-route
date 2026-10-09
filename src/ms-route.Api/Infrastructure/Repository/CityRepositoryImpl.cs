using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Infrastructure.Persistence.Context;
using ms_route.Api.Infrastructure.Persistence.Entity;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.Repository;

public class CityRepositoryImpl : ICityRepository
{
    private readonly RouteContext _context;

    public CityRepositoryImpl(RouteContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<City>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _context.Cities
            .AsNoTracking()
            .Where(c => c.Status == "Active")
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        return entities.Select(c => new City { Id = c.Id, Name = c.Name }).ToList();
    }
}
