using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GetCurrentTripServiceTests
{
    // 2026-10-21 12:00 UTC = 07:00 en America/Bogota.
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task ExecuteAsync_ConViajeActivo_RetornaElViaje()
    {
        var driverId = Guid.NewGuid();
        var routeId = Guid.NewGuid();
        var executionRepo = new InMemoryRouteExecutionRepository();
        executionRepo.Executions.Add(new RouteExecution
        {
            Id = Guid.NewGuid(),
            RouteId = routeId,
            BusId = Guid.NewGuid(),
            DriverId = driverId,
            StartDateTime = Now.UtcDateTime,
            Status = Status.Active
        });

        var service = new GetCurrentTripService(executionRepo, TestMapper.Create(), new FixedTimeProvider(Now));

        var result = await service.ExecuteAsync(driverId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.DriverId, Is.EqualTo(driverId));
        Assert.That(result.RouteId, Is.EqualTo(routeId));
        Assert.That(result.Status, Is.EqualTo("Active"));
        Assert.That(result.EndDateTime, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_SinViajeActivo_RetornaNull()
    {
        var driverId = Guid.NewGuid();
        var executionRepo = new InMemoryRouteExecutionRepository();
        executionRepo.Executions.Add(new RouteExecution
        {
            Id = Guid.NewGuid(),
            BusId = Guid.NewGuid(),
            DriverId = driverId,
            StartDateTime = Now.UtcDateTime.AddHours(-2),
            EndDateTime = Now.UtcDateTime.AddHours(-1),
            Status = Status.Inactive
        });

        var service = new GetCurrentTripService(executionRepo, TestMapper.Create(), new FixedTimeProvider(Now));

        var result = await service.ExecuteAsync(driverId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_ConViajeDeOtroDriver_RetornaNull()
    {
        var executionRepo = new InMemoryRouteExecutionRepository();
        executionRepo.Executions.Add(new RouteExecution
        {
            Id = Guid.NewGuid(),
            BusId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            StartDateTime = Now.UtcDateTime,
            Status = Status.Active
        });

        var service = new GetCurrentTripService(executionRepo, TestMapper.Create(), new FixedTimeProvider(Now));

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    // Un viaje que nunca se cerro no debe seguir reportandose como vigente al
    // dia siguiente: era el origen de los viajes activos "fantasma".
    [Test]
    public async Task ExecuteAsync_ConViajeActivoDeDiasAnteriores_RetornaNull()
    {
        var driverId = Guid.NewGuid();
        var executionRepo = new InMemoryRouteExecutionRepository();
        executionRepo.Executions.Add(new RouteExecution
        {
            Id = Guid.NewGuid(),
            BusId = Guid.NewGuid(),
            DriverId = driverId,
            StartDateTime = Now.UtcDateTime.AddDays(-2),
            Status = Status.Active
        });

        var service = new GetCurrentTripService(executionRepo, TestMapper.Create(), new FixedTimeProvider(Now));

        var result = await service.ExecuteAsync(driverId);

        Assert.That(result, Is.Null);
    }
}
