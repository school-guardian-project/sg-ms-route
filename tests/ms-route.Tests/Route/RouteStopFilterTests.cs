using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using NUnit.Framework;

namespace ms_route.Tests.Route;

public class RouteStopFilterTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void FilterStops_ResolvesAssignmentByRouteStopId_NotPhysicalStopId(bool isParent)
    {
        var profileId = Guid.NewGuid();
        var home = new RouteStop { Id = Guid.NewGuid(), StopId = Guid.NewGuid(), OrderSequence = 1, Status = Status.Active };
        var school = new RouteStop { Id = Guid.NewGuid(), StopId = Guid.NewGuid(), OrderSequence = 3, Status = Status.Active };
        var other = new RouteStop { Id = Guid.NewGuid(), StopId = Guid.NewGuid(), OrderSequence = 2, Status = Status.Active };
        var assignment = new RouteStudentAssignment { ProfileId = profileId, RouteStopId = home.Id, Status = Status.Active };
        var stops = new[] { home, other, school };
        var byId = stops.ToDictionary(stop => stop.StopId, stop => new ms_route.Api.Domain.Model.Stop { Id = stop.StopId });

        var result = new RouteStopFilter().FilterStops(stops, byId, assignment, isParent, profileId).ToList();

        Assert.That(result.Select(stop => stop.Id), Is.EquivalentTo(
            isParent ? new[] { home.Id, school.Id } : new[] { home.Id }));
        Assert.That(result.Any(stop => stop.Id == other.Id), Is.False);
    }
}
