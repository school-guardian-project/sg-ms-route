using System.Net;
using System.Text.Json;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Infrastructure.External;

public class FleetService : IFleetService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public FleetService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Guid?> GetBusIdAssignedToDriverAsync(Guid profileId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/buses/assigned/{profileId}", ct);
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            if (!response.IsSuccessStatusCode) return null;

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var bus = await JsonSerializer.DeserializeAsync<BusAssignedResponse>(stream, JsonOptions, ct);
            return bus?.Id;
        }
        catch (HttpRequestException)
        {
            // ms-fleet caido no debe tumbar la consulta de la ruta del conductor.
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return null;
        }
    }

    private sealed record BusAssignedResponse(Guid Id, string Plate, Guid CampuseId);
}
