using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;
using StopModel = ms_route.Api.Domain.Model.Stop;

namespace ms_route.Tests.Fakes;

public class InMemoryStopRepository : IStopRepository
{
    public readonly List<StopModel> Stops = new();

    public Task<StopModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Stops.FirstOrDefault(s => s.Id == id));

    public Task<IReadOnlyList<StopModel>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToHashSet();
        return Task.FromResult<IReadOnlyList<StopModel>>(Stops.Where(s => idList.Contains(s.Id)).ToList());
    }

    public Task<IReadOnlyList<StopModel>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<StopModel>>(Stops.ToList());

    public Task<StopModel> SaveAsync(StopModel stop, CancellationToken ct = default)
    {
        Stops.Add(stop);
        return Task.FromResult(stop);
    }

    public Task UpdateAsync(StopModel stop, CancellationToken ct = default)
    {
        var existing = Stops.FirstOrDefault(s => s.Id == stop.Id)
            ?? throw new InvalidOperationException($"Stop not found: {stop.Id}");

        existing.Name = stop.Name;
        existing.Address = stop.Address;
        existing.Latitude = stop.Latitude;
        existing.Longitude = stop.Longitude;
        existing.Status = stop.Status;

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
