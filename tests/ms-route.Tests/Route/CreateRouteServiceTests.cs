using ms_route.Api.Application.Dto;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CreateRouteServiceTests
{
    [Test]
    public async Task ExecuteAsync_ConRutaValida_CreaRuta()
    {
        var repo = new InMemoryRouteRepository();
        var service = new CreateRouteService(repo, TestMapper.Create());

        var result = await service.ExecuteAsync(new RouteRequestDto
        {
            CampuseId = Guid.NewGuid(),
            Name = "R-01",
            TargetSector = "Norte"
        });

        Assert.That(repo.Routes, Has.Exactly(1).Items);
        Assert.That(result.Name, Is.EqualTo("R-01"));
    }

    [Test]
    public async Task ExecuteAsync_ConNombreDuplicado_Rechaza()
    {
        var repo = new InMemoryRouteRepository();
        repo.Routes.Add(new RouteModel { Id = Guid.NewGuid(), Name = "R-01", TargetSector = "Norte", Status = Status.Active });
        var service = new CreateRouteService(repo, TestMapper.Create());

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(new RouteRequestDto
            {
                CampuseId = Guid.NewGuid(),
                Name = "R-01",
                TargetSector = "Sur"
            }));
    }

    [Test]
    public async Task ExecuteAsync_SinNombre_Rechaza()
    {
        var repo = new InMemoryRouteRepository();
        var service = new CreateRouteService(repo, TestMapper.Create());

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync(new RouteRequestDto
            {
                CampuseId = Guid.NewGuid(),
                Name = "  ",
                TargetSector = "Norte"
            }));
    }
}
