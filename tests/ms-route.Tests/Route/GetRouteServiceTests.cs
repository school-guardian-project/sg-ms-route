using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using Xunit;
using RouteModel = ms_route.Api.Domain.Model.Route;

namespace ms_route.Tests.Route;

public class GetRouteServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ConRutaExistente_RetornaRuta()
    {
        var repo = new InMemoryRouteRepository();
        var routeId = Guid.NewGuid();
        repo.Routes.Add(new RouteModel { Id = routeId, Name = "R-01", TargetSector = "Norte", Status = RouteStatus.Active });
        var service = new GetRouteService(repo);

        var result = await service.ExecuteAsync(routeId);

        Assert.Equal(routeId, result.Id);
        Assert.Equal("R-01", result.Name);
    }

    [Fact]
    public async Task ExecuteAsync_ConRutaInexistente_LanzaExcepcion()
    {
        var repo = new InMemoryRouteRepository();
        var service = new GetRouteService(repo);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(Guid.NewGuid()));

        Assert.Contains("not found", ex.Message);
    }
}
