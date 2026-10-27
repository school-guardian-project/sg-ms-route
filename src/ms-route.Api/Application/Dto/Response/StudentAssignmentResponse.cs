namespace ms_route.Api.Application.Dto.Response;

/// <summary>
/// Response DTO for student assignment.
/// </summary>
/// <param name="Id">Assignment identifier.</param>
/// <param name="RouteId">Route identifier.</param>
/// <param name="StudentId">Student identifier.</param>
/// <param name="StopId">Stop identifier.</param>
/// <param name="Status">Assignment status as string.</param>
public sealed record StudentAssignmentResponse(
    Guid Id,
    Guid RouteId,
    Guid StudentId,
    Guid StopId,
    string Status);