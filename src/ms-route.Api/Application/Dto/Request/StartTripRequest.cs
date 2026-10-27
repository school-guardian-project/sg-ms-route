using System.ComponentModel.DataAnnotations;

namespace ms_route.Api.Application.Dto.Request;

/// <summary>
/// Request DTO for starting a trip.
/// </summary>
public sealed class StartTripRequest
{
    [Required]
    public Guid RouteId { get; init; }

    [Required]
    public Guid BusId { get; init; }

    /// <summary>
    /// Optional: the driver always comes from the access token. Kept for
    /// compatibility with clients that still send it; a different id is 403.
    /// </summary>
    public Guid? DriverId { get; init; }
}
