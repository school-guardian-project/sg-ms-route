using System.ComponentModel.DataAnnotations;

namespace ms_route.Api.Application.Dto.Request;

/// <summary>
/// Request DTO for creating a new route.
/// </summary>
public sealed class CreateRouteRequest
{
    [Required]
    public Guid CampuseId { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string TargetSector { get; init; } = string.Empty;

    [Required]
    public TimeOnly StartTime { get; init; }

    [Required]
    public TimeOnly EndTime { get; init; }

    public List<string> DaysOfWeek { get; init; } = new();
}