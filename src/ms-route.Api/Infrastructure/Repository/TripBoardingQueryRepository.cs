using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Port.Out;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;
using ms_route.Api.Infrastructure.Persistence.Entity;
using ms_route.Api.Domain.Model;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ms_route.Api.Infrastructure.Repository;

/// <summary>
/// Implementación del puerto ITripBoardingQuery usando EF Core.
/// Consulta información de embarques/desembarques desde la base de datos.
/// </summary>
public sealed class TripBoardingQueryRepository : ITripBoardingQuery
{
    private readonly RouteContext _context;

    public TripBoardingQueryRepository(RouteContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<int> GetTotalAssignedStudentsAsync(Guid routeId, CancellationToken ct = default)
    {
        return await _context.RouteStudentAssignments
            .AsNoTracking()
            .Where(rsa => rsa.Status == Status.Active)
            .Join(_context.RouteStops.AsNoTracking().Where(rs => rs.RouteId == routeId && rs.Status == Status.Active),
                rsa => rsa.RouteStopId,
                rs => rs.Id,
                (rsa, rs) => rsa.ProfileId)
            .Distinct()
            .CountAsync(ct);
    }

    /// <inheritdoc />
    public async Task<int> GetAlightedCountAtStopAsync(Guid tripId, Guid routeStopId, CancellationToken ct = default)
    {
        try
        {
            return await _context.Set<BoardingEntity>()
                .AsNoTracking()
                .Where(b => b.RouteExecutionId == tripId 
                         && b.RouteStopId == routeStopId 
                         && b.BoardingType == BoardingType.OFF_BOARD)
                .CountAsync(ct);
        }
        catch
        {
            return 0;
        }
    }

    /// <inheritdoc />
    public async Task<Guid?> GetFinalRouteStopIdAsync(Guid routeId, string direction, CancellationToken ct = default)
    {
        var query = _context.RouteStops
            .AsNoTracking()
            .Where(rs => rs.RouteId == routeId && rs.Status == Status.Active);

        var finalStop = await query
            .OrderByDescending(rs => rs.OrderSequence)
            .Select(rs => rs.Id)
            .FirstOrDefaultAsync(ct);

        return finalStop == Guid.Empty ? null : finalStop;
    }

    /// <inheritdoc />
    public async Task<bool> HasReachedFinalStopAsync(Guid tripId, Guid finalRouteStopId, CancellationToken ct = default)
    {
        try
        {
            var hasScan = await _context.Set<BoardingEntity>()
                .AsNoTracking()
                .AnyAsync(b => b.RouteExecutionId == tripId && b.RouteStopId == finalRouteStopId, ct);
            
            return hasScan;
        }
        catch
        {
            var execution = await _context.RouteExecutions
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == tripId && e.Status == Status.Active, ct);
            
            return execution != null;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetStopNameAsync(Guid routeStopId, CancellationToken ct = default)
    {
        var name = await _context.RouteStops
            .AsNoTracking()
            .Where(rs => rs.Id == routeStopId)
            .Join(_context.Stops.AsNoTracking(),
                rs => rs.StopId,
                s => s.Id,
                (rs, s) => s.Name)
            .FirstOrDefaultAsync(ct);

        return name ?? "Parada Final";
    }
}

file enum BoardingType { ON_BOARD, OFF_BOARD }
file class BoardingEntity
{
    public Guid Id { get; set; }
    public Guid RouteExecutionId { get; set; }
    public Guid ProfileId { get; set; }
    public Guid RouteStopId { get; set; }
    public DateTime DateTime { get; set; }
    public BoardingType BoardingType { get; set; }
}