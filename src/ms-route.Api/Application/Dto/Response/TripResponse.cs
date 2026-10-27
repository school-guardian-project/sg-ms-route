namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for trip.
/// </summary>
/// <param name="Id">Trip identifier.</param>
/// <param name="RouteId">Route identifier.</param>
/// <param name="BusId">Bus identifier.</param>
/// <param name="DriverId">Driver identifier.</param>
/// <param name="StartDateTime">Trip start date/time.</param>
/// <param name="EndDateTime">Trip end date/time.</param>
/// <param name="Status">Trip status as string.</param>
/// <param name="Direction">Trip direction.</param>
public sealed record TripResponse(
    Guid Id,
    Guid RouteId,
    Guid BusId,
    Guid DriverId,
    DateTime StartDateTime,
    DateTime? EndDateTime,
    string Status,
    string Direction);