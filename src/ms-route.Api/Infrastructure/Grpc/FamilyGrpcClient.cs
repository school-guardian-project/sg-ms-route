using Grpc.Net.Client;
using ms_route.Api.Domain.Ports.Out;
using ms_route.Api.Infrastructure.Grpc.Family;

namespace ms_route.Api.Infrastructure.Grpc;

/// <summary>
/// Cliente gRPC del FamilyService de ms-user-management: resuelve los
/// acudientes de un estudiante para autorizar la consulta de su ruta.
/// </summary>
public sealed class FamilyGrpcClient : IStudentGuardianDirectory, IDisposable
{
    // Holgura amplia: la primera llamada tras un arranque en frio puede tardar
    // varios segundos en el servicio destino.
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    private readonly GrpcChannel _channel;
    private readonly FamilyService.FamilyServiceClient _client;

    public FamilyGrpcClient(IConfiguration configuration)
    {
        _channel = GrpcChannel.ForAddress(
            configuration["Family:GrpcAddress"]
            ?? "http://ms-user-management:5001");
        _client = new FamilyService.FamilyServiceClient(_channel);
    }

    public async Task<IReadOnlyCollection<Guid>> GetGuardianProfileIdsAsync(
        Guid studentProfileId, CancellationToken ct = default)
    {
        var response = await _client.GetFamilyMembersByStudentAsync(
            new GetFamilyMembersByStudentRequest { StudentProfileId = studentProfileId.ToString() },
            deadline: DateTime.UtcNow.Add(Timeout),
            cancellationToken: ct);

        return response.ParentProfileIds
            .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();
    }

    public void Dispose() => _channel.Dispose();
}
