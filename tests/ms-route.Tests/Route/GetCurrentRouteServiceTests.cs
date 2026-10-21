using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using NUnit.Framework;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GetCurrentRouteServiceTests
{
    // 2026-10-21 12:00 UTC = 07:00 en America/Bogota.
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task ExecuteAsync_ConEjecucionActiva_RetornaLaRutaDeLaEjecucion()
    {
        var driverId = Guid.NewGuid();
        var otherRouteId = Guid.NewGuid();
        var executionRouteId = Guid.NewGuid();

        var routeRepo = new InMemoryRouteRepository();
        routeRepo.Routes.Add(new RouteModel { Id = otherRouteId, Name = "Otra ruta" });
        routeRepo.Routes.Add(new RouteModel { Id = executionRouteId, Name = "Ruta del conductor" });

        var executionRepo = new InMemoryRouteExecutionRepository();
        executionRepo.Executions.Add(new RouteExecution
        {
            Id = Guid.NewGuid(),
            RouteId = executionRouteId,
            BusId = Guid.NewGuid(),
            DriverId = driverId,
            StartDateTime = Now.UtcDateTime,
            Status = Status.Active
        });

        var service = new GetCurrentRouteService(
            routeRepo,
            executionRepo,
            new InMemoryRouteStopRepository(),
            new InMemoryStopRepository(),
            TestMapper.Create(),
            new FixedTimeProvider(Now));

        var result = await service.ExecuteAsync(driverId);

        Assert.That(result.Id, Is.EqualTo(executionRouteId));
        Assert.That(result.Stops, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_SinEjecucionActiva_LanzaExcepcion()
    {
        var service = new GetCurrentRouteService(
            new InMemoryRouteRepository(),
            new InMemoryRouteExecutionRepository(),
            new InMemoryRouteStopRepository(),
            new InMemoryStopRepository(),
            TestMapper.Create(),
            new FixedTimeProvider(Now));

        Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(Guid.NewGuid()));
    }
}
