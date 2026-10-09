using Microsoft.AspNetCore.Mvc;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;

namespace ms_route.Api.Infrastructure.Controller;

[ApiController]
[Route("api/routes")]
public class RouteAssignmentController : ControllerBase
{
    private readonly IAssignStudentToRouteUseCase _assignStudentToRouteUseCase;
    private readonly IGetStudentStopOnRouteUseCase _getStudentStopOnRouteUseCase;
    private readonly IAttachStopToRouteUseCase _attachStopToRouteUseCase;
    private readonly IAssignBusToRouteUseCase _assignBusToRouteUseCase;

    public RouteAssignmentController(
        IAssignStudentToRouteUseCase assignStudentToRouteUseCase,
        IGetStudentStopOnRouteUseCase getStudentStopOnRouteUseCase,
        IAttachStopToRouteUseCase attachStopToRouteUseCase,
        IAssignBusToRouteUseCase assignBusToRouteUseCase)
    {
        _assignStudentToRouteUseCase = assignStudentToRouteUseCase;
        _getStudentStopOnRouteUseCase = getStudentStopOnRouteUseCase;
        _attachStopToRouteUseCase = attachStopToRouteUseCase;
        _assignBusToRouteUseCase = assignBusToRouteUseCase;
    }

    [HttpPost("{routeId:guid}/stops")]
    public async Task<IActionResult> AttachStop(
        Guid routeId,
        [FromBody] AttachStopRequest request,
        CancellationToken ct)
    {
        var result = await _attachStopToRouteUseCase.ExecuteAsync(routeId, request.StopId,
            ct);
        return Ok(result);
    }

    [HttpPost("{routeId:guid}/students")]
    public async Task<IActionResult> AssignStudent(
        Guid routeId,
        [FromBody] AssignStudentRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _assignStudentToRouteUseCase.ExecuteAsync(
                routeId, request.StudentId, request.StopId, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{routeId:guid}/bus")]
    public async Task<IActionResult> AssignBus(
        Guid routeId,
        [FromBody] AssignBusRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _assignBusToRouteUseCase.ExecuteAsync(routeId, request.BusId, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{routeId:guid}/students/{studentProfileId:guid}")]
    public async Task<IActionResult> GetStudentStop(
        Guid routeId,
        Guid studentProfileId,
        CancellationToken ct)
    {
        var result = await _getStudentStopOnRouteUseCase.ExecuteAsync(routeId, studentProfileId, ct);

        return result is null ? NotFound() : Ok(result);
    }

    public class AssignStudentRequest
    {
        public Guid StudentId { get; set; }
        public Guid StopId { get; set; }
    }

    public class AttachStopRequest
    {
        public Guid StopId { get; set; }
    }

    public class AssignBusRequest
    {
        public Guid BusId { get; set; }
    }
}
