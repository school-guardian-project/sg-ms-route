using ms_route.Api.Application.Dto;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Tests.Fakes;
using NUnit.Framework;
using StopModel = ms_route.Api.Domain.Model.Stop;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GetRouteSuggestionServiceTests
{
    private readonly Guid _routeId = Guid.NewGuid();
    private readonly FakeMapRoutingService _routing = new();
    private readonly InMemoryRouteRepository _routes = new();
    private readonly InMemoryRouteStopRepository _routeStops = new();
    private readonly InMemoryStopRepository _stops = new();
    private readonly GetRouteSuggestionUseCase _useCase;

    public GetRouteSuggestionServiceTests()
    {
        // Ruta con paradas
        _routes.Routes.Add(new ms_route.Api.Domain.Model.Route
        {
            Id = _routeId,
            Name = "Ruta Norte",
            StartTime = new TimeOnly(6, 30),
            EndTime = new TimeOnly(10, 0),
            Status = ms_route.Api.Domain.Model.Status.Active
        });

        // Dos paradas en orden
        var stop1 = new StopModel { Id = Guid.NewGuid(), Name = "Parada A", Address = "Calle 1", Latitude = 2.9m, Longitude = -75.2m, Status = ms_route.Api.Domain.Model.Status.Active };
        var stop2 = new StopModel { Id = Guid.NewGuid(), Name = "Parada B", Address = "Calle 2", Latitude = 2.95m, Longitude = -75.3m, Status = ms_route.Api.Domain.Model.Status.Active };

        _routeStops.RouteStops.Add(new RouteStop { Id = Guid.NewGuid(), RouteId = _routeId, StopId = stop1.Id, OrderSequence = 1, Status = ms_route.Api.Domain.Model.Status.Active });
        _routeStops.RouteStops.Add(new RouteStop { Id = Guid.NewGuid(), RouteId = _routeId, StopId = stop2.Id, OrderSequence = 2, Status = ms_route.Api.Domain.Model.Status.Active });

        _stops.Stops.Add(stop1);
        _stops.Stops.Add(stop2);

        _useCase = new GetRouteSuggestionUseCase(
            _routes,
            _routing,
            _routeStops,
            _stops);
    }

    [Test]
    public async Task ExecuteAsync_WithRouteAndStops_ReturnsSuggestion()
    {
        // Act
        var result = await _useCase.ExecuteAsync(_routeId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RouteId, Is.EqualTo(_routeId));
        Assert.That(result.Stops, Is.Not.Null);
        Assert.That(result.Stops.Count, Is.EqualTo(2));
        // Cuando el fake devuelve geometría, Geometry debe estar poblado
        Assert.That(result.Geometry, Is.Not.Null);
        Assert.That(result.Geometry!.Points.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task ExecuteAsync_WithRoute_WithoutStops_ReturnsEmptySuggestion()
    {
        // Arrang
        var emptyRouteId = Guid.NewGuid();
        _routes.Routes.Add(new ms_route.Api.Domain.Model.Route
        {
            Id = emptyRouteId,
            Name = "Ruta Vacía",
            StartTime = new TimeOnly(6, 30),
            EndTime = new TimeOnly(10, 0),
            Status = ms_route.Api.Domain.Model.Status.Active
        });

        // Act
        var result = await _useCase.ExecuteAsync(emptyRouteId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RouteId, Is.EqualTo(emptyRouteId));
        Assert.That(result.Stops, Is.Not.Null);
        Assert.That(result.Stops.Count, Is.EqualTo(0));
        // Geometry debe ser null cuando no hay paradas
        Assert.That(result.Geometry, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_WithSingleStop_ReturnsNullGeometry()
    {
        // Arrang: ruta con una sola parada
        var singleRouteId = Guid.NewGuid();
        var singleStopId = Guid.NewGuid();
        _routes.Routes.Add(new ms_route.Api.Domain.Model.Route
        {
            Id = singleRouteId,
            Name = "Ruta Simple",
            StartTime = new TimeOnly(6, 30),
            EndTime = new TimeOnly(10, 0),
            Status = ms_route.Api.Domain.Model.Status.Active
        });
        _stops.Stops.Add(new StopModel
        {
            Id = singleStopId,
            Name = "Parada Unica",
            Address = "Calle 3",
            Latitude = 3.0m,
            Longitude = -75.4m,
            Status = ms_route.Api.Domain.Model.Status.Active
        });
        _routeStops.RouteStops.Add(new RouteStop
        {
            Id = Guid.NewGuid(),
            RouteId = singleRouteId,
            StopId = singleStopId,
            OrderSequence = 1,
            Status = ms_route.Api.Domain.Model.Status.Active
        });

        // Act
        var result = await _useCase.ExecuteAsync(singleRouteId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RouteId, Is.EqualTo(singleRouteId));
        Assert.That(result.Stops, Has.Count.EqualTo(1));
        // Con una sola parada no hay trazado vial
        Assert.That(result.Geometry, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_WithRoutingNull_ReturnsNullGeometry()
    {
        // Arrang: enrutador que siempre devuelve null
        var routing = new FakeMapRoutingService(returnNull: true);
        var useCase = new GetRouteSuggestionUseCase(
            _routes,
            routing,
            _routeStops,
            _stops);

        // Act
        var result = await useCase.ExecuteAsync(_routeId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RouteId, Is.EqualTo(_routeId));
        Assert.That(result.Stops, Is.Not.Null);
        // Geometry es null porque el proveedor no respondió
        Assert.That(result.Geometry, Is.Null);
    }
}