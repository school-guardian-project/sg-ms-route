using ms_route.Api.Application.UseCase;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using RouteModel = ms_route.Api.Domain.Model.Route;
using Xunit;

namespace ms_route.Tests.Route;

public class ListRouteServiceTenantTests
{
    private static readonly Guid CampusA = Guid.NewGuid();
    private static readonly Guid CampusB = Guid.NewGuid();
    private static readonly Guid SchoolId = Guid.NewGuid();

    private static InMemoryRouteRepository BuildRepo()
    {
        var repo = new InMemoryRouteRepository();
        repo.Routes.Add(new RouteModel { Id = Guid.NewGuid(), Name = "R-A", CampuseId = CampusA, TargetSector = "Norte", Status = Status.Active });
        repo.Routes.Add(new RouteModel { Id = Guid.NewGuid(), Name = "R-B", CampuseId = CampusB, TargetSector = "Sur", Status = Status.Active });
        return repo;
    }

    private static ListRouteService BuildService(
        InMemoryRouteRepository repo,
        FakeTenantProvider tenant,
        InMemorySchoolCampusRepository? campusRepo = null)
        => new(repo, new InMemoryRouteStopRepository(),
            campusRepo ?? new InMemorySchoolCampusRepository(),
            tenant, TestMapper.Create());

    [Fact]
    public async Task ExecuteAsync_AdminConSchoolId_SoloRutasDeLasSedesDelColegio()
    {
        var campusRepo = new InMemorySchoolCampusRepository();
        campusRepo.CampusesBySchool[SchoolId] = [CampusA];
        var service = BuildService(BuildRepo(),
            new FakeTenantProvider { RoleId = 1, SchoolId = SchoolId, ShouldFilter = true }, campusRepo);

        var result = (await service.ExecuteAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("R-A", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ParentConCampusId_SoloRutasDeSuSede()
    {
        var service = BuildService(BuildRepo(),
            new FakeTenantProvider { RoleId = 4, CampusId = CampusB, ShouldFilter = true });

        var result = (await service.ExecuteAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("R-B", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_SinFiltro_RetornaTodas()
    {
        var service = BuildService(BuildRepo(), new FakeTenantProvider { ShouldFilter = false });

        var result = (await service.ExecuteAsync()).ToList();

        Assert.Equal(2, result.Count);
    }
}
