using Microsoft.AspNetCore.Mvc;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;

namespace ms_route.Api.Infrastructure.Controller;

[ApiController]
[Route("api/routes")]
public class RouteAssignmentController : ControllerBase
{
    private readonly IAssignStudentToRouteUseCase _assignStudentToRouteUseCase;

    public RouteAssignmentController(IAssignStudentToRouteUseCase assignStudentToRouteUseCase)
    {
        _assignStudentToRouteUseCase = assignStudentToRouteUseCase;
    }

    [HttpPost("{routeId:guid}/students")]
    public async Task<IActionResult> AssignStudent(
        Guid routeId,
        [FromBody] AssignStudentRequest request,
        CancellationToken ct)
    {
        var result = await _assignStudentToRouteUseCase.ExecuteAsync(
            routeId, request.StudentId, request.StopId, ct);
        return Ok(result);
    }

    public class AssignStudentRequest
    {
        public Guid StudentId { get; set; }
        public Guid StopId { get; set; }
    }
}
