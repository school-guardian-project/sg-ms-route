using ms_route.Api.Application.Dto;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using Xunit;

namespace ms_route.Tests.Route;

public class UpdateRouteServiceTests
{
    [Fact]
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
        Assert.Equal("R-01-Updated", updated.Name);
    }

    [Fact]
    public async Task ExecuteAsync_ConRutaInexistente_LanzaExcepcion()
    {
        var repo = new InMemoryRouteRepository();
        var service = new UpdateRouteService(repo, TestMapper.Create());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid(), new RouteRequestDto
            {
                CampuseId = Guid.NewGuid(),
                Name = "R-01",
                TargetSector = "Norte"
            }));
    }
}
