using Microsoft.EntityFrameworkCore;
using ms_route.Api.Infrastructure.Persistence.Context;
using ms_route.Api.Infrastructure.Persistence.Entity;
using NUnit.Framework;

namespace ms_route.Tests.Persistence;

[TestFixture]
public sealed class RouteSchemaMetadataTests
{
    [Test]
    public void RouteNameDoesNotHaveAnUnbackedUniqueIndex()
    {
        var options = new DbContextOptionsBuilder<RouteContext>()
            .UseSqlServer("Server=localhost;Database=metadata-only;User Id=unused;Password=unused;TrustServerCertificate=True")
            .Options;

        using var context = new RouteContext(options);
        var route = context.Model.FindEntityType(typeof(RouteEntity))!;

        Assert.That(
            route.GetIndexes().Any(index =>
                index.IsUnique &&
                index.Properties.Count == 1 &&
                index.Properties[0].Name == nameof(RouteEntity.Name)),
            Is.False);
    }

    [Test]
    public void RouteExecutionDirectionMatchesTheAvailableSqlDefinition()
    {
        var options = new DbContextOptionsBuilder<RouteContext>()
            .UseSqlServer("Server=localhost;Database=metadata-only;User Id=unused;Password=unused;TrustServerCertificate=True")
            .Options;

        using var context = new RouteContext(options);
        var direction = context.Model.FindEntityType(typeof(RouteExecutionEntity))!
            .FindProperty(nameof(RouteExecutionEntity.Direction))!;

        Assert.Multiple(() =>
        {
            Assert.That(direction.IsNullable, Is.False);
            Assert.That(direction.GetMaxLength(), Is.EqualTo(10));
            Assert.That(direction.IsUnicode(), Is.False);
            Assert.That(direction.GetDefaultValue(), Is.EqualTo("Ida"));
        });
    }

    [Test]
    public void RouteDaysOfWeekIsStoredAsRequiredTextWithAnEmptyArrayDefault()
    {
        var options = new DbContextOptionsBuilder<RouteContext>()
            .UseSqlServer("Server=localhost;Database=metadata-only;User Id=unused;******;TrustServerCertificate=True")
            .Options;

        using var context = new RouteContext(options);
        var daysOfWeek = context.Model.FindEntityType(typeof(RouteEntity))!
            .FindProperty(nameof(RouteEntity.DaysOfWeek))!;

        Assert.Multiple(() =>
        {
            Assert.That(daysOfWeek.GetColumnType(), Is.EqualTo("nvarchar(max)"));
            Assert.That(daysOfWeek.IsNullable, Is.False);
            Assert.That(daysOfWeek.GetDefaultValueSql(), Is.EqualTo("N'[]'"));
        });
    }
}
