using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ms_route.Api.Infrastructure.Controller;
using ms_route.Api.Infrastructure.DependencyInjection;
using NUnit.Framework;

namespace ms_route.Tests.Route;

public class RouteDetailDependencyTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void StudentRouteController_ResolvesWithDefaultInternalServiceAddresses_WithoutOpeningDatabase(bool fleetConfigured)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=route-test;Integrated Security=true;TrustServerCertificate=true"
        }).Build();
        if (fleetConfigured)
            configuration["Fleet:GrpcAddress"] = "http://127.0.0.1:5001";
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddRouteServices(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.DoesNotThrow(() => ActivatorUtilities.CreateInstance<RouteDetailController>(scope.ServiceProvider));
    }
}
