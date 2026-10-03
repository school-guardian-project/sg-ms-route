using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using Xunit;

namespace ms_route.Tests.Route;

public class GetCurrentTripServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ConViajeActivo_RetornaElViaje()
    {
        var driverId = Guid.NewGuid();
        var executionRepo = new InMemoryRouteExecutionRepository();
        executionRepo.Executions.Add(new RouteExecution
        {
            Id = Guid.NewGuid(),
            BusId = Guid.NewGuid(),
            DriverId = driverId,
            StartDateTime = DateTime.UtcNow,
            Status = Status.Active
        });

        var service = new GetCurrentTripService(executionRepo, TestMapper.Create());

        var result = await service.ExecuteAsync(driverId);

        Assert.NotNull(result);
        Assert.Equal(driverId, result.DriverId);
        Assert.Equal("Active", result.Status);
        Assert.Null(result.EndDateTime);
    }

    [Fact]
    public async Task ExecuteAsync_SinViajeActivo_RetornaNull()
    {
        var driverId = Guid.NewGuid();
        var executionRepo = new InMemoryRouteExecutionRepository();
        executionRepo.Executions.Add(new RouteExecution
        {
            Id = Guid.NewGuid(),
            BusId = Guid.NewGuid(),
            DriverId = driverId,
            StartDateTime = DateTime.UtcNow.AddHours(-2),
            EndDateTime = DateTime.UtcNow.AddHours(-1),
            Status = Status.Inactive
        });

        var service = new GetCurrentTripService(executionRepo, TestMapper.Create());

        var result = await service.ExecuteAsync(driverId);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_ConViajeDeOtroDriver_RetornaNull()
    {
        var executionRepo = new InMemoryRouteExecutionRepository();
        executionRepo.Executions.Add(new RouteExecution
        {
            Id = Guid.NewGuid(),
            BusId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            StartDateTime = DateTime.UtcNow,
            Status = Status.Active
        });

        var service = new GetCurrentTripService(executionRepo, TestMapper.Create());

        var result = await service.ExecuteAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
