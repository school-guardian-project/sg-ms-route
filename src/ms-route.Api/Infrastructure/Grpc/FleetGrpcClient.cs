using Grpc.Core;
using Grpc.Net.Client;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Infrastructure.Grpc;

/// <summary>
/// gRPC client for ms-fleet's bus lookups (contract route.v1.RouteService).
/// Inter-service traffic is gRPC-only (ADR-005); the shared secret travels in
/// the x-internal-api-key metadata header and comes from configuration.
/// </summary>
public sealed class FleetGrpcClient : IFleetService, IDisposable
{
    // Holgura amplia: la primera llamada tras un arranque en frio puede tardar
    // varios segundos en el servicio destino.
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    private readonly RouteService.RouteServiceClient _client;
    private readonly string _apiKey;
    private readonly GrpcChannel? _channel;

    public FleetGrpcClient(IConfiguration configuration)
    {
        var address = configuration["Fleet:GrpcAddress"]
            ?? "http://ms-fleet:5001";
        _channel = GrpcChannel.ForAddress(address);
        _client = new RouteService.RouteServiceClient(_channel);
        _apiKey = configuration["Fleet:ApiKey"] ?? string.Empty;
    }

    public async Task<BusInfo?> GetBusAssignedToDriverAsync(Guid profileId, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetBusAssignedToDriverAsync(
                new GetBusAssignedToDriverRequest { ProfileId = profileId.ToString() },
                Headers(), Deadline(), ct);

            // Solo hace falta el eslabon conductor -> bus: la placa basta para
            // resolver la ruta de ms-route (el conductor es el propio solicitante).
            return new BusInfo
            {
                Id = ToGuid(response.BusId),
                Plate = response.Plate
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            // Paridad con el puente REST anterior: 404 desde ms-fleet => no asignado.
            return null;
        }
    }

    public async Task<BusInfo?> GetBusDetailsAsync(Guid busId, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetBusDetailsAsync(
                new GetBusDetailsRequest { BusId = busId.ToString() },
                Headers(), Deadline(), ct);

            return new BusInfo
            {
                // Si ms-fleet no devuelve id util conservamos el conocido.
                Id = ToGuidOrNull(response.BusId) ?? busId,
                Plate = response.Plate,
                Capacity = response.Capacity,
                DriverProfileId = ToGuidOrNull(response.DriverProfileId),
                DriverName = response.DriverName
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    private Metadata Headers() => new() { { "x-internal-api-key", _apiKey } };

    private static DateTime Deadline() => DateTime.UtcNow.Add(Timeout);

    private static Guid ToGuid(string value) => ToGuidOrNull(value) ?? Guid.Empty;

    private static Guid? ToGuidOrNull(string value)
        => Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;

    public void Dispose() => _channel?.Dispose();
}
