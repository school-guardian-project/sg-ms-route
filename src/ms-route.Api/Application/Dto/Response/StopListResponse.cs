namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for stop list item.
/// </summary>
/// <param name="Id">Stop identifier.</param>
/// <param name="Name">Stop name.</param>
/// <param name="Address">Stop address.</param>
/// <param name="Latitude">Latitude.</param>
/// <param name="Longitude">Longitude.</param>
/// <param name="CityId">City identifier.</param>
/// <param name="SchoolId">School identifier.</param>
/// <param name="RouteId">Route identifier.</param>
/// <param name="RouteName">Route name.</param>
/// <param name="RouteNames">List of route names.</param>
public sealed record StopListResponse(
    Guid Id,
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    Guid CityId,
    Guid SchoolId,
    Guid? RouteId,
    string RouteName,
    List<string> RouteNames);