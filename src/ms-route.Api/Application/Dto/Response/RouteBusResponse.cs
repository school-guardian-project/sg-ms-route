namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for route bus with driver info.
/// </summary>
/// <param name="Id">Bus identifier.</param>
/// <param name="Plate">Bus plate.</param>
/// <param name="Capacity">Bus capacity.</param>
/// <param name="DriverProfileId">Driver profile identifier.</param>
/// <param name="DriverName">Driver name.</param>
public sealed record RouteBusResponse(
    Guid Id,
    string Plate,
    int Capacity,
    Guid? DriverProfileId,
    string DriverName);