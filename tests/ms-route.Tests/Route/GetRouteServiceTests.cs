using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using StopModel = ms_route.Api.Domain.Model.Stop;
using Xunit;

namespace ms_route.Tests.Route;

public class GetRouteServiceTests
{
    private static GetRouteService CreateService(InMemoryRouteRepository repo) =>
        new(
            repo,
            new InMemoryRouteBusAssignmentRepository(),
            new InMemoryRouteStopRepository(),
            new InMemoryStopRepository(),
            TestMapper.Create());

    [Fact]
    public async Task ExecuteAsync_ConRutaExistente_RetornaRuta()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", TargetSector = "Norte", Status = Status.Active });
        var service = CreateService(repo);

        var result = await service.ExecuteAsync(routeId);

        Assert.Equal(routeId, result.Id);
        Assert.Equal("R-01", result.Name);
    }

    [Fact]
    public async Task ExecuteAsync_ConRutaInexistente_LanzaExcepcion()
    {
        var repo = new InMemoryRouteRepository();
        var service = CreateService(repo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid()));
    }

    [Fact]
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

        Assert.Equal(busId, result.BusId);
        var stop = Assert.Single(result.Stops);
        Assert.Equal(stopId, stop.Id);
        Assert.Equal("Parada 1", stop.Name);
        Assert.Equal(1, stop.OrderSequence);
    }

    [Fact]
    public async Task ExecuteAsync_SinBusAsignado_RetornaBusIdNulo()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", Status = Status.Active });
        var service = CreateService(repo);

        var result = await service.ExecuteAsync(routeId);

        Assert.Null(result.BusId);
        Assert.Empty(result.Stops);
    }
}
