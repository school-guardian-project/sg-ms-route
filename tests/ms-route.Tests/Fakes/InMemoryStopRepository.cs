using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class InMemoryStopRepository : IStopRepository
{
    public readonly List<Stop> Stops = new();

    public Task<Stop?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Stops.FirstOrDefault(s => s.Id == id));

    public Task<IReadOnlyList<Stop>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Stop>>(Stops.ToList());

    public Task<Stop> SaveAsync(Stop stop, CancellationToken ct = default)
    {
        Stops.Add(stop);
        return Task.FromResult(stop);
    }

    public Task UpdateAsync(Stop stop, CancellationToken ct = default)
    {
        var existing = Stops.FirstOrDefault(s => s.Id == stop.Id)
            ?? throw new InvalidOperationException($"Stop not found: {stop.Id}");

        existing.Name = stop.Name;
        existing.Address = stop.Address;
        existing.Latitude = stop.Latitude;
        existing.Longitude = stop.Longitude;
        existing.Status = stop.Status;
        existing.UpdatedAt = stop.UpdatedAt;

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = Stops.FirstOrDefault(s => s.Id == id)
            ?? throw new InvalidOperationException($"Stop not found: {id}");

        Stops.Remove(existing);
        return Task.CompletedTask;
    }
}
