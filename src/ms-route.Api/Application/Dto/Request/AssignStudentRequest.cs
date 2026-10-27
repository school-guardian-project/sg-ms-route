using System.ComponentModel.DataAnnotations;

namespace ms_route.Api.Application.Dto.Request;

/// <summary>
/// Request DTO for assigning a student to a route.
/// </summary>
public sealed class AssignStudentRequest
{
    [Required]
    public Guid StudentId { get; init; }

    [Required]
    public Guid StopId { get; init; }
}