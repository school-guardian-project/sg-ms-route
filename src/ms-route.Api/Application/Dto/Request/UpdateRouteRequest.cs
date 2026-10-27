using System.ComponentModel.DataAnnotations;

namespace ms_route.Api.Application.Dto.Request;

/// <summary>
/// Request DTO for updating an existing route.
/// All properties are optional - only provided values will be updated.
/// </summary>
public sealed class UpdateRouteRequest
{
    public Guid? CampuseId { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }

    [StringLength(50, MinimumLength = 1)]
    public string? TargetSector { get; init; }

    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }

    public List<string>? DaysOfWeek { get; init; }
}