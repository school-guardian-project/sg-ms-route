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

    [Required]
    public Guid DriverId { get; init; }
}