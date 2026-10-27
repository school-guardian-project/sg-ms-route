namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for bus assignment.
/// </summary>
/// <param name="Id">Assignment identifier.</param>
/// <param name="RouteId">Route identifier.</param>
/// <param name="BusId">Bus identifier.</param>
/// <param name="Status">Assignment status as string.</param>
public sealed record BusAssignmentResponse(
    Guid Id,
    Guid RouteId,
    Guid BusId,
    string Status);