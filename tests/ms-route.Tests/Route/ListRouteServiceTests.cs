using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class ListRouteServiceTests
{
    [Test]
    public async Task ExecuteAsync_ConRutas_RetornaLista()
    {
        var repo = new InMemoryRouteRepository();
        repo.Routes.Add(new RouteModel { Id = Guid.NewGuid(), Name = "R-01", TargetSector = "Norte", Status = Status.Active });
        repo.Routes.Add(new RouteModel { Id = Guid.NewGuid(), Name = "R-02", TargetSector = "Sur", Status = Status.Active });
        var service = new ListRouteService(repo, new InMemoryRouteStopRepository(), new InMemorySchoolCampusRepository(), new FakeTenantProvider(), TestMapper.Create());

        var result = await service.ExecuteAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task ExecuteAsync_SinRutas_RetornaListaVacia()
    {
        var repo = new InMemoryRouteRepository();
        var service = new ListRouteService(repo, new InMemoryRouteStopRepository(), new InMemorySchoolCampusRepository(), new FakeTenantProvider(), TestMapper.Create());

        var result = await service.ExecuteAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_ConRutas_IncluyeDestinoYCantidadDeParadas()
    {
        var repo = new InMemoryRouteRepository();
        var routeNorte = new RouteModel { Id = Guid.NewGuid(), Name = "R-01", TargetSector = "Norte", Status = Status.Active };
        var routeSur = new RouteModel { Id = Guid.NewGuid(), Name = "R-02", TargetSector = "Sur", Status = Status.Active };
        repo.Routes.Add(routeNorte);
        repo.Routes.Add(routeSur);

        var stopRepo = new InMemoryRouteStopRepository();
        stopRepo.RouteStops.Add(new RouteStop { RouteId = routeNorte.Id });
        stopRepo.RouteStops.Add(new RouteStop { RouteId = routeNorte.Id });
        stopRepo.RouteStops.Add(new RouteStop { RouteId = routeNorte.Id, Status = Status.Inactive });

        var service = new ListRouteService(repo, stopRepo, new InMemorySchoolCampusRepository(), new FakeTenantProvider(), TestMapper.Create());

        var result = (await service.ExecuteAsync()).ToList();

        Assert.That(result.Single(r => r.Id == routeNorte.Id).TargetSector, Is.EqualTo("Norte"));
        Assert.That(result.Single(r => r.Id == routeNorte.Id).StopsCount, Is.EqualTo(2));
        Assert.That(result.Single(r => r.Id == routeSur.Id).StopsCount, Is.EqualTo(0));
    }
}
