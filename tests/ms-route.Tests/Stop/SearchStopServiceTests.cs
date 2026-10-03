using ms_route.Api.Application.Search.Stop;
using ms_route.Api.Application.UseCase;
using ms_route.Tests.Fakes;
using StopModel = ms_route.Api.Domain.Model.Stop;
using Xunit;

namespace ms_route.Tests.Stop;

public class SearchStopServiceTests
{
    private static SearchStopService CreateService(InMemoryStopRepository stopRepo)
    {
        var strategies = new List<IStopSearchStrategy>
        {
            new NameSearchStrategy()
        };

        return new SearchStopService(new ListStopService(stopRepo, TestMapper.Create()), strategies);
    }

    [Fact]
    public async Task ExecuteAsync_TerminoVacio_RetornaListaVacia()
    {
        var repo = new InMemoryStopRepository();
        repo.Stops.Add(new StopModel { Id = Guid.NewGuid(), Name = "Parada Norte", Address = "Av. Libertad 123" });
        var service = CreateService(repo);

        var result = await service.ExecuteAsync("");

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_TerminoPorNombre_FiltraIgnorandoMayusculas()
    {
        var repo = new InMemoryStopRepository();
        var norte = new StopModel
        {
            Id = Guid.NewGuid(),
            Name = "Parada Norte",
            Address = "Av. Libertad 123",
            Latitude = -33.45m,
            Longitude = -70.66m
        };
        var sur = new StopModel
        {
            Id = Guid.NewGuid(),
            Name = "Parada Sur",
            Address = "Calle Falsa 123",
            Latitude = -33.60m,
            Longitude = -70.50m
        };
        repo.Stops.Add(norte);
        repo.Stops.Add(sur);

        var service = CreateService(repo);

        var result = (await service.ExecuteAsync("parada NORTE")).ToList();

        var matched = Assert.Single(result);
        Assert.Equal(norte.Id, matched.Id);
        Assert.Equal("Av. Libertad 123", matched.Address);
    }

    [Fact]
    public async Task ExecuteAsync_TerminoPorDireccionOCoordenada_Filtra()
    {
        var repo = new InMemoryStopRepository();
        var norte = new StopModel
        {
            Id = Guid.NewGuid(),
            Name = "Parada Norte",
            Address = "Av. Libertad 123",
            Latitude = -33.45m,
            Longitude = -70.66m
        };
        repo.Stops.Add(norte);

        var service = CreateService(repo);

        var porDireccion = (await service.ExecuteAsync("LIBERTAD")).ToList();
        Assert.Equal(norte.Id, Assert.Single(porDireccion).Id);

        var porCoordenada = (await service.ExecuteAsync("-33.45")).ToList();
        Assert.Equal(norte.Id, Assert.Single(porCoordenada).Id);
    }
}
