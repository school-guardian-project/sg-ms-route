using ms_route.Api.Application.Search.Stop;
using ms_route.Api.Application.UseCase;
using ms_route.Tests.Fakes;
using StopModel = ms_route.Api.Domain.Model.Stop;
using NUnit.Framework;

namespace ms_route.Tests.Stop;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SearchStopServiceTests
{
    private static SearchStopService CreateService(InMemoryStopRepository stopRepo)
    {
        var strategies = new List<IStopSearchStrategy>
        {
            new NameSearchStrategy()
        };

        return new SearchStopService(new ListStopService(stopRepo, new InMemoryRouteStopRepository(), new InMemoryRouteRepository(), TestMapper.Create()), strategies);
    }

    [Test]
    public async Task ExecuteAsync_TerminoVacio_RetornaListaVacia()
    {
        var repo = new InMemoryStopRepository();
        repo.Stops.Add(new StopModel { Id = Guid.NewGuid(), Name = "Parada Norte", Address = "Av. Libertad 123" });
        var service = CreateService(repo);

        var result = await service.ExecuteAsync("");

        Assert.That(result, Is.Empty);
    }

    [Test]
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

        var matchedItems = result;
        Assert.That(matchedItems, Has.Exactly(1).Items);
        var matched = matchedItems.Single();
        Assert.That(matched.Id, Is.EqualTo(norte.Id));
        Assert.That(matched.Address, Is.EqualTo("Av. Libertad 123"));
    }

    [Test]
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
        Assert.That(porDireccion.Single().Id, Is.EqualTo(norte.Id));

        var porCoordenada = (await service.ExecuteAsync("-33.45")).ToList();
        Assert.That(porCoordenada.Single().Id, Is.EqualTo(norte.Id));
    }
}
