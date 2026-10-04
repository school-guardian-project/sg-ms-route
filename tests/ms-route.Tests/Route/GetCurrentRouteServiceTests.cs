using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using Xunit;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Tests.Route;

public class GetCurrentRouteServiceTests
{
    [Fact]
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
            StartDateTime = DateTime.UtcNow,
            Status = Status.Active
        });

        var service = new GetCurrentRouteService(
            routeRepo,
            executionRepo,
            new InMemoryRouteStopRepository(),
            new InMemoryStopRepository(),
            TestMapper.Create());

        var result = await service.ExecuteAsync(driverId);

        Assert.Equal(executionRouteId, result.Id);
        Assert.Empty(result.Stops);
    }

    [Fact]
    public async Task ExecuteAsync_SinEjecucionActiva_LanzaExcepcion()
    {
        var service = new GetCurrentRouteService(
            new InMemoryRouteRepository(),
            new InMemoryRouteExecutionRepository(),
            new InMemoryRouteStopRepository(),
            new InMemoryStopRepository(),
            TestMapper.Create());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(Guid.NewGuid()));
    }
}
