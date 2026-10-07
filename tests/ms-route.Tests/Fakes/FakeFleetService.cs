using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class FakeFleetService : IFleetService
{
    public Guid? BusId { get; set; }

    public Task<Guid?> GetBusIdAssignedToDriverAsync(Guid profileId, CancellationToken ct = default)
        => Task.FromResult(BusId);
}
