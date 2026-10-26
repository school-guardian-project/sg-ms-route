namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for stop detail.
/// </summary>
/// <param name="Id">Stop identifier.</param>
/// <param name="Name">Stop name.</param>
/// <param name="Address">Stop address.</param>
/// <param name="Latitude">Latitude.</param>
/// <param name="Longitude">Longitude.</param>
/// <param name="OrderSequence">Order sequence in route.</param>
public sealed record StopDetailResponse(
    Guid Id,
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    int OrderSequence);