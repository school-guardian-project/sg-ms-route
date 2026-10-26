using System.ComponentModel.DataAnnotations;

namespace ms_route.Api.Application.Dto.Request;

/// <summary>
/// Request DTO for attaching a stop to a route.
/// </summary>
public sealed class AttachStopRequest
{
    [Required]
    public Guid StopId { get; init; }
}