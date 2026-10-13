using ms_route.Api.Application.Dto;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class UpdateRouteServiceTests
{
    [Test]
    public async Task ExecuteAsync_ConRutaExistente_ActualizaRuta()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", TargetSector = "Norte", Status = Status.Active });
        var service = new UpdateRouteService(repo, TestMapper.Create());

        await service.ExecuteAsync(routeId, new RouteRequestDto
        {
            CampuseId = Guid.NewGuid(),
            Name = "R-01-Updated",
            TargetSector = "Sur"
        });

        var updated = repo.Routes.First(r => r.Id == routeId);
        Assert.That(updated.Name, Is.EqualTo("R-01-Updated"));
    }

    [Test]
    public async Task ExecuteAsync_ConRutaInexistente_LanzaExcepcion()
    {
        var repo = new InMemoryRouteRepository();
        var service = new UpdateRouteService(repo, TestMapper.Create());

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid(), new RouteRequestDto
            {
                CampuseId = Guid.NewGuid(),
                Name = "R-01",
                TargetSector = "Norte"
            }));
    }
}
