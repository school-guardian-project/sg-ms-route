using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Service;
using NUnit.Framework;

namespace ms_route.Tests.Route;

public class TripStartWindowTests
{
    private static TripStartWindow EarlyWindow(TimeOnly start) =>
        TripStartWindow.Create(
            start.Add(-TripLifecycleService.EarlyStart),
            TripLifecycleService.DefaultStartGracePeriod + TripLifecycleService.EarlyStart);

    [TestCase(5, 49, false)]
    [TestCase(5, 50, true)]
    [TestCase(6, 0, true)]
    [TestCase(6, 20, true)]
    [TestCase(6, 21, false)]
    public void IsWithinWindow_OpensTenMinutesBeforeStart(int hour, int minute, bool expected)
    {
        var window = EarlyWindow(new TimeOnly(6, 0));

        Assert.That(window.IsWithinWindow(new TimeOnly(hour, minute)), Is.EqualTo(expected));
    }
}
