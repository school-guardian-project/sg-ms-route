using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class FakeFleetService : IFleetService
{
    /// <summary>Bus que ms-fleet devuelve para cualquier conductor.</summary>
    public BusInfo? AssignedBus { get; set; }

    /// <summary>Detalle que ms-fleet devuelve por id de bus.</summary>
    public BusInfo? BusDetails { get; set; }

    public Task<BusInfo?> GetBusAssignedToDriverAsync(Guid profileId, CancellationToken ct = default)
        => Task.FromResult(AssignedBus);

    public Task<BusInfo?> GetBusDetailsAsync(Guid busId, CancellationToken ct = default)
        => Task.FromResult(BusDetails);
}
