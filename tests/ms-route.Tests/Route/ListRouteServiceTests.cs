using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using Xunit;

namespace ms_route.Tests.Route;

public class ListRouteServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ConRutas_RetornaLista()
    {
        var repo = new InMemoryRouteRepository();
        repo.Routes.Add(new RouteModel { Id = Guid.NewGuid(), Name = "R-01", TargetSector = "Norte", Status = Status.Active });
        repo.Routes.Add(new RouteModel { Id = Guid.NewGuid(), Name = "R-02", TargetSector = "Sur", Status = Status.Active });
        var service = new ListRouteService(repo, new InMemoryRouteStopRepository(), TestMapper.Create());

        var result = await service.ExecuteAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ExecuteAsync_SinRutas_RetornaListaVacia()
    {
        var repo = new InMemoryRouteRepository();
        var service = new ListRouteService(repo, new InMemoryRouteStopRepository(), TestMapper.Create());

        var result = await service.ExecuteAsync();

        Assert.Empty(result);
    }

    [Fact]
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

        var service = new ListRouteService(repo, stopRepo, TestMapper.Create());

        var result = (await service.ExecuteAsync()).ToList();

        Assert.Equal("Norte", result.Single(r => r.Id == routeNorte.Id).TargetSector);
        Assert.Equal(2, result.Single(r => r.Id == routeNorte.Id).StopsCount);
        Assert.Equal(0, result.Single(r => r.Id == routeSur.Id).StopsCount);
    }
}
