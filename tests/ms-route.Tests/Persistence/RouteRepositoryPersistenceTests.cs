using Microsoft.EntityFrameworkCore;
using ms_route.Api.Domain.Model;
using ms_route.Api.Infrastructure.Persistence.Context;
using ms_route.Api.Infrastructure.Repository;
using NUnit.Framework;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Tests.Persistence;

[TestFixture]
public sealed class RouteRepositoryPersistenceTests
{
    [Test]
    public async Task SaveAndRead_PersistsConfiguredDaysOfWeek()
    {
        await using var context = CreateContext();
        var repository = new RouteRepositoryImpl(context);
        var route = new RouteModel
        {
            Id = Guid.NewGuid(),
            CampuseId = Guid.NewGuid(),
            Name = "R-01",
            TargetSector = "North",
            StartTime = new TimeOnly(6, 30),
            EndTime = new TimeOnly(7, 30),
            DaysOfWeek = ["Monday", "Wednesday"]
        };

        await repository.SaveAsync(route);
        var saved = await repository.GetByIdAsync(route.Id);

        Assert.That(saved?.DaysOfWeek, Is.EqualTo(new[] { "Monday", "Wednesday" }));
    }

    [Test]
    public async Task Update_PersistsChangedDaysOfWeek()
    {
        await using var context = CreateContext();
        var repository = new RouteRepositoryImpl(context);
        var route = new RouteModel
        {
            Id = Guid.NewGuid(),
            CampuseId = Guid.NewGuid(),
            Name = "R-01",
            TargetSector = "North",
            StartTime = new TimeOnly(6, 30),
            EndTime = new TimeOnly(7, 30),
            DaysOfWeek = ["Monday"]
        };
        await repository.SaveAsync(route);

        route.DaysOfWeek = ["Tuesday", "Thursday"];
        await repository.UpdateAsync(route);
        var updated = await repository.GetByIdAsync(route.Id);

        Assert.That(updated?.DaysOfWeek, Is.EqualTo(new[] { "Tuesday", "Thursday" }));
    }

    private static RouteContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RouteContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RouteContext(options);
    }
}
