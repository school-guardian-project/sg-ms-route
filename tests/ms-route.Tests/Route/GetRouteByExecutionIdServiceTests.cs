using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using NUnit.Framework;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GetRouteByExecutionIdServiceTests
{
    private readonly InMemoryRouteExecutionRepository _executions = new();
    private readonly InMemoryRouteRepository _routes = new();

    [Test]
    public async Task ReturnsTheRouteReferencedByTheExecutionIncludingHistoricalTrips()
    {
        var route = new RouteModel { Id = Guid.NewGuid(), Name = "School route", Status = Status.Inactive };
        _routes.Routes.Add(route);
        var execution = new RouteExecution { Id = Guid.NewGuid(), RouteId = route.Id, Status = Status.Inactive };
        _executions.Executions.Add(execution);

        var result = await new GetRouteByExecutionIdService(_executions, _routes).ExecuteAsync(execution.Id);

        Assert.That(result, Is.SameAs(route));
    }

    [Test]
    public async Task MissingExecutionReturnsNullRatherThanAnUnrelatedRoute()
    {
        _routes.Routes.Add(new RouteModel());
        var result = await new GetRouteByExecutionIdService(_executions, _routes).ExecuteAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task MissingReferencedRouteReturnsNull()
    {
        var execution = new RouteExecution { Id = Guid.NewGuid(), RouteId = Guid.NewGuid() };
        _executions.Executions.Add(execution);
        var result = await new GetRouteByExecutionIdService(_executions, _routes).ExecuteAsync(execution.Id);
        Assert.That(result, Is.Null);
    }
}
