using ms_route.Api.Application.Search.Route;
using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using Xunit;

namespace ms_route.Tests.Route;

public class SearchRouteServiceTests
{
    private static SearchRouteService CreateService(
        InMemoryRouteRepository routeRepo,
        InMemoryRouteStopRepository routeStopRepo)
    {
        var strategies = new List<IRouteSearchStrategy>
        {
            new ScheduleSearchStrategy(),
            new NameSearchStrategy()
        };

        return new SearchRouteService(
            new ListRouteService(routeRepo, routeStopRepo, TestMapper.Create()),
            strategies);
    }

    [Fact]
    public async Task ExecuteAsync_TerminoVacio_RetornaListaVacia()
    {
        var repo = new InMemoryRouteRepository();
        repo.Routes.Add(new RouteModel { Id = Guid.NewGuid(), Name = "R-01", TargetSector = "Norte" });
        var service = CreateService(repo, new InMemoryRouteStopRepository());

        var result = await service.ExecuteAsync("   ");

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_TerminoHora_FiltraPorHorario()
    {
        var repo = new InMemoryRouteRepository();
        var temprana = new RouteModel
        {
            Id = Guid.NewGuid(),
            Name = "R-01",
            TargetSector = "Norte",
            StartTime = new TimeOnly(7, 30),
            EndTime = new TimeOnly(17, 0)
        };
        var tarde = new RouteModel
        {
            Id = Guid.NewGuid(),
            Name = "R-02",
            TargetSector = "Sur",
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(18, 30)
        };
        repo.Routes.Add(temprana);
        repo.Routes.Add(tarde);

        var routeStopRepo = new InMemoryRouteStopRepository();
        routeStopRepo.RouteStops.Add(new RouteStop { RouteId = temprana.Id });

        var service = CreateService(repo, routeStopRepo);

        var result = (await service.ExecuteAsync("07:30")).ToList();

        var matched = Assert.Single(result);
        Assert.Equal(temprana.Id, matched.Id);
        Assert.Equal(new TimeOnly(7, 30), matched.StartTime);
        Assert.Equal(1, matched.StopsCount);
        Assert.False(new ScheduleSearchStrategy().CanHandle("R-02"));
    }

    [Fact]
    public async Task ExecuteAsync_TerminoGenerico_FiltraPorNombreYSector()
    {
        var repo = new InMemoryRouteRepository();
        var routeNorte = new RouteModel { Id = Guid.NewGuid(), Name = "R-01", TargetSector = "Norte" };
        var routeSur = new RouteModel { Id = Guid.NewGuid(), Name = "R-02", TargetSector = "Sur" };
        repo.Routes.Add(routeNorte);
        repo.Routes.Add(routeSur);

        var service = CreateService(repo, new InMemoryRouteStopRepository());

        var porSector = (await service.ExecuteAsync("norte")).ToList();
        Assert.Equal(routeNorte.Id, Assert.Single(porSector).Id);

        var porNombre = (await service.ExecuteAsync("r-02")).ToList();
        Assert.Equal(routeSur.Id, Assert.Single(porNombre).Id);
    }
}
