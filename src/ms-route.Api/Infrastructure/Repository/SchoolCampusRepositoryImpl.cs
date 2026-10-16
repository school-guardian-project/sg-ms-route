using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Infrastructure.Persistence.Context;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.Repository;

public class SchoolCampusRepositoryImpl : ISchoolCampusRepository
{
    private readonly RouteContext _context;

    public SchoolCampusRepositoryImpl(RouteContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Guid>> GetCampusIdsBySchoolAsync(Guid schoolId, CancellationToken ct = default)
    {
        return await _context.SchoolCampuses
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId)
            .Select(c => c.Id)
            .ToListAsync(ct);
    }
}
