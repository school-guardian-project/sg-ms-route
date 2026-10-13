using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using StopModel = ms_route.Api.Domain.Model.Stop;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GetRouteServiceTests
{
    private static GetRouteService CreateService(InMemoryRouteRepository repo) =>
        new(
            repo,
            new InMemoryRouteBusAssignmentRepository(),
            new InMemoryRouteStopRepository(),
            new InMemoryStopRepository(),
            TestMapper.Create());

    [Test]
    public async Task ExecuteAsync_ConRutaExistente_RetornaRuta()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", TargetSector = "Norte", Status = Status.Active });
        var service = CreateService(repo);

        var result = await service.ExecuteAsync(routeId);

        Assert.That(result.Id, Is.EqualTo(routeId));
        Assert.That(result.Name, Is.EqualTo("R-01"));
    }

    [Test]
    public async Task ExecuteAsync_ConRutaInexistente_LanzaExcepcion()
    {
        var repo = new InMemoryRouteRepository();
        var service = CreateService(repo);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task ExecuteAsync_ConBusAsignadoYParadas_RetornaBusIdYStops()
    {
        var routeId = Guid.NewGuid();
        var busId = Guid.NewGuid();
        var stopId = Guid.NewGuid();

        var repo = new InMemoryRouteRepository();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", Status = Status.Active });

        var busRepo = new InMemoryRouteBusAssignmentRepository();
        busRepo.Assignments.Add(new RouteBusAssignment
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            BusId = busId,
            Status = Status.Active
        });

        var routeStops = new InMemoryRouteStopRepository();
        routeStops.RouteStops.Add(new RouteStop
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            StopId = stopId,
            OrderSequence = 1,
            Status = Status.Active
        });

        var stops = new InMemoryStopRepository();
        stops.Stops.Add(new StopModel
        {
            Id = stopId,
            Name = "Parada 1",
            Address = "Calle 1",
            Latitude = 4.7m,
            Longitude = -74.1m
        });

        var service = new GetRouteService(repo, busRepo, routeStops, stops, TestMapper.Create());

        var result = await service.ExecuteAsync(routeId);

        Assert.That(result.BusId, Is.EqualTo(busId));
        var stopItems = result.Stops;
        Assert.That(stopItems, Has.Exactly(1).Items);
        var stop = stopItems.Single();
        Assert.That(stop.Id, Is.EqualTo(stopId));
        Assert.That(stop.Name, Is.EqualTo("Parada 1"));
        Assert.That(stop.OrderSequence, Is.EqualTo(1));
    }

    [Test]
    public async Task ExecuteAsync_SinBusAsignado_RetornaBusIdNulo()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", Status = Status.Active });
        var service = CreateService(repo);

        var result = await service.ExecuteAsync(routeId);

        Assert.That(result.BusId, Is.Null);
        Assert.That(result.Stops, Is.Empty);
    }
}
