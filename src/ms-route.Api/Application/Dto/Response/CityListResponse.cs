namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for city list item.
/// </summary>
/// <param name="Id">City identifier.</param>
/// <param name="Name">City name.</param>
public sealed record CityListResponse(
    Guid Id,
    string Name);