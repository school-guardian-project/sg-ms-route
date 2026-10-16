using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class UnassignBusFromRouteServiceTests
{
    [Test]
    public async Task ExecuteAsync_ConBusAsignado_LoDesasigna()
    {
        var routeId = Guid.NewGuid();
        var assignment = new RouteBusAssignment
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            BusId = Guid.NewGuid(),
            Status = Status.Active
        };

        var assignments = new InMemoryRouteBusAssignmentRepository();
        assignments.Assignments.Add(assignment);

        var service = new UnassignBusFromRouteService(assignments);

        await service.ExecuteAsync(routeId);

        Assert.That(assignment.Status, Is.EqualTo(Status.Inactive));
        Assert.That(assignments.RouteHasBusAsync(routeId).Result, Is.False);
    }

    [Test]
    public async Task ExecuteAsync_SinBusAsignado_LanzaExcepcion()
    {
        var service = new UnassignBusFromRouteService(new InMemoryRouteBusAssignmentRepository());

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid()));
    }
}
