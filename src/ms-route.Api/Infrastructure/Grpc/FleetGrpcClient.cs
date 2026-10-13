using Grpc.Core;
using Grpc.Net.Client;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Infrastructure.Grpc;

/// <summary>
/// gRPC client for ms-fleet's bus assignment service (contract route.v1.RouteService).
/// Inter-service traffic is gRPC-only (ADR-005); the shared secret travels in
/// the x-internal-api-key metadata header and comes from configuration.
/// </summary>
public sealed class FleetGrpcClient : IFleetService, IDisposable
{
    // Holgura amplia: la primera llamada tras un arranque en frio puede tardar
    // varios segundos en el servicio destino.
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);
    private const string ApiKeyHeader = "x-internal-api-key";

    private readonly RouteService.RouteServiceClient _client;
    private readonly string _apiKey;
    private readonly GrpcChannel? _channel;

    public FleetGrpcClient(IConfiguration configuration)
    {
        var address = configuration["Fleet:GrpcAddress"]
            ?? throw new InvalidOperationException("Missing Fleet:GrpcAddress (set it in .env)");
        _channel = GrpcChannel.ForAddress(address);
        _client = new RouteService.RouteServiceClient(_channel);
        _apiKey = configuration["Fleet:ApiKey"] ?? string.Empty;
    }

    public async Task<Guid?> GetBusIdAssignedToDriverAsync(Guid profileId, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetBusIdAssignedToDriverAsync(
                new GetBusIdAssignedToDriverRequest { ProfileId = profileId.ToString() },
                new Metadata { { "x-internal-api-key", _apiKey } }, Deadline(), ct);
            return Guid.Parse(response.BusId);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            // Paridad con el puente REST anterior: 404 desde ms-fleet => no asignado.
            return null;
        }
    }

    private static DateTime Deadline() => DateTime.UtcNow.Add(Timeout);

    public void Dispose() => _channel?.Dispose();
}