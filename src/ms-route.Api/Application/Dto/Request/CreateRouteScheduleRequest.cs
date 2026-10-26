using System.ComponentModel.DataAnnotations;

namespace ms_route.Api.Application.Dto.Request;

/// <summary>
/// Request DTO for creating a route schedule.
/// </summary>
public sealed class CreateRouteScheduleRequest
{
    /// <summary>Weekday name as stored by the database (Monday..Friday).</summary>
    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string DayOfWeek { get; init; } = string.Empty;

    /// <summary>Direction of the trip (e.g. "Ida" or "Vuelta"). Max 10 characters.</summary>
    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string Direction { get; init; } = string.Empty;

    [Required]
    public TimeOnly StartTime { get; init; }
}