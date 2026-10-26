namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for stop.
/// </summary>
/// <param name="Id">Stop identifier.</param>
/// <param name="Name">Stop name.</param>
/// <param name="Address">Stop address.</param>
/// <param name="Latitude">Latitude.</param>
/// <param name="Longitude">Longitude.</param>
/// <param name="CityId">City identifier as string.</param>
/// <param name="SchoolId">School identifier as string.</param>
/// <param name="Status">Stop status as string.</param>
public sealed record StopResponse(
    Guid Id,
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    string CityId,
    string SchoolId,
    string Status);