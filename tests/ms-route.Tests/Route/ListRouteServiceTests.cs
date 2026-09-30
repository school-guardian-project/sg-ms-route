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
        var service = new ListRouteService(repo, TestMapper.Create());

        var result = await service.ExecuteAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ExecuteAsync_SinRutas_RetornaListaVacia()
    {
        var repo = new InMemoryRouteRepository();
        var service = new ListRouteService(repo, TestMapper.Create());

        var result = await service.ExecuteAsync();

        Assert.Empty(result);
    }
}
