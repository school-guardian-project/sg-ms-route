using System.Net;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ms_route.Api.Infrastructure.Grpc;
using NUnit.Framework;

namespace ms_route.Tests.Route;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class FleetGrpcClientTests
{
    [TestCase(null)]
    [TestCase(StatusCode.NotFound)]
    [TestCase(StatusCode.Unauthenticated)]
    [TestCase(StatusCode.Unavailable)]
    public async Task AssignedDriverLookup_ReadsFullBusDetails_AndDoesNotHideServiceFailures(StatusCode? detailError)
    {
        var stub = new FleetLookupStub { DetailError = detailError };
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(options =>
            options.Listen(IPAddress.Loopback, 0, listen => listen.Protocols = HttpProtocols.Http2));
        builder.Services.AddGrpc();
        builder.Services.AddSingleton(stub);
        await using var app = builder.Build();
        app.MapGrpcService<FleetLookupStub>();
        await app.StartAsync();
        try
        {
            var addresses = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses;
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fleet:GrpcAddress"] = addresses.Single(),
                ["Fleet:ApiKey"] = "local-regression-key"
            }).Build();
            using var client = new FleetGrpcClient(configuration);

            if (detailError is StatusCode.Unauthenticated or StatusCode.Unavailable)
            {
                var exception = Assert.ThrowsAsync<RpcException>(
                    () => client.GetBusAssignedToDriverAsync(stub.ProfileId));
                Assert.That(exception!.StatusCode, Is.EqualTo(detailError));
            }
            else
            {
                var bus = await client.GetBusAssignedToDriverAsync(stub.ProfileId);
                if (detailError == StatusCode.NotFound)
                    Assert.That(bus, Is.Null);
                else
                {
                    Assert.That(bus, Is.Not.Null);
                    Assert.Multiple(() =>
                    {
                        Assert.That(bus!.Id, Is.EqualTo(stub.BusId));
                        Assert.That(bus.Plate, Is.EqualTo("ABC123"));
                        Assert.That(bus.Capacity, Is.EqualTo(40));
                        Assert.That(bus.DriverProfileId, Is.EqualTo(stub.ProfileId));
                        Assert.That(bus.DriverName, Is.EqualTo("Test Driver"));
                    });
                }
            }
            Assert.That(stub.Calls, Is.EqualTo(new[] { "assignment", "details" }));
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [BindServiceMethod(typeof(FleetLookupStub), nameof(BindService))]
    public class FleetLookupStub
    {
        public static void BindService(ServiceBinderBase binder, FleetLookupStub? service)
        {
            binder.AddMethod(new Method<GetBusAssignedToDriverRequest, BusAssignedResponse>(
                MethodType.Unary, "route.v1.RouteService", "GetBusAssignedToDriver",
                Marshallers.Create(request => request.ToByteArray(), GetBusAssignedToDriverRequest.Parser.ParseFrom),
                Marshallers.Create(response => response.ToByteArray(), BusAssignedResponse.Parser.ParseFrom)),
                service is null ? null : service.GetBusAssignedToDriver);
            binder.AddMethod(new Method<GetBusDetailsRequest, BusDetailsResponse>(
                MethodType.Unary, "route.v1.RouteService", "GetBusDetails",
                Marshallers.Create(request => request.ToByteArray(), GetBusDetailsRequest.Parser.ParseFrom),
                Marshallers.Create(response => response.ToByteArray(), BusDetailsResponse.Parser.ParseFrom)),
                service is null ? null : service.GetBusDetails);
        }

        public Guid ProfileId { get; } = Guid.NewGuid();
        public Guid BusId { get; } = Guid.NewGuid();
        public StatusCode? DetailError { get; init; }
        public List<string> Calls { get; } = [];

        private static void CheckMetadata(ServerCallContext context)
        {
            if (context.RequestHeaders.GetValue("x-internal-api-key") != "local-regression-key")
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing test API key"));
            if (context.Deadline <= DateTime.UtcNow || context.Deadline == DateTime.MaxValue)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Missing deadline"));
        }

        public virtual Task<BusAssignedResponse> GetBusAssignedToDriver(
            GetBusAssignedToDriverRequest request, ServerCallContext context)
        {
            CheckMetadata(context);
            if (request.ProfileId != ProfileId.ToString())
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Unexpected profile"));
            Calls.Add("assignment");
            return Task.FromResult(new BusAssignedResponse { BusId = BusId.ToString(), Plate = "ABC123" });
        }

        public virtual Task<BusDetailsResponse> GetBusDetails(GetBusDetailsRequest request, ServerCallContext context)
        {
            CheckMetadata(context);
            if (request.BusId != BusId.ToString())
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Unexpected bus"));
            Calls.Add("details");
            if (DetailError.HasValue)
                throw new RpcException(new Status(DetailError.Value, "Controlled test error"));
            return Task.FromResult(new BusDetailsResponse
            {
                BusId = BusId.ToString(), Plate = "ABC123", Capacity = 40,
                DriverProfileId = ProfileId.ToString(), DriverName = "Test Driver"
            });
        }
    }
}
