using System.ComponentModel.DataAnnotations;

namespace ms_route.Api.Application.Dto.Request;

/// <summary>
/// Request DTO for assigning a bus to a route.
/// </summary>
public sealed class AssignBusRequest
{
    [Required]
    public Guid BusId { get; init; }
}