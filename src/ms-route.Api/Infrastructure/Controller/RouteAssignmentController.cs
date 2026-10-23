using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Infrastructure.Auth;

namespace ms_route.Api.Infrastructure.Controller;

[ApiController]
[Route("api/routes")]
[RequireProfile]
public class RouteAssignmentController : ControllerBase
{
    private readonly IAssignStudentToRouteUseCase _assignStudentToRouteUseCase;
    private readonly IGetStudentStopOnRouteUseCase _getStudentStopOnRouteUseCase;
    private readonly IAttachStopToRouteUseCase _attachStopToRouteUseCase;
    private readonly IAssignBusToRouteUseCase _assignBusToRouteUseCase;
    private readonly IUnassignBusFromRouteUseCase _unassignBusFromRouteUseCase;
    private readonly ICreateRouteScheduleUseCase _createRouteScheduleUseCase;
    private readonly IListRouteSchedulesUseCase _listRouteSchedulesUseCase;

    public RouteAssignmentController(
        IAssignStudentToRouteUseCase assignStudentToRouteUseCase,
        IGetStudentStopOnRouteUseCase getStudentStopOnRouteUseCase,
        IAttachStopToRouteUseCase attachStopToRouteUseCase,
        IAssignBusToRouteUseCase assignBusToRouteUseCase,
        IUnassignBusFromRouteUseCase unassignBusFromRouteUseCase,
        ICreateRouteScheduleUseCase createRouteScheduleUseCase,
        IListRouteSchedulesUseCase listRouteSchedulesUseCase)
    {
        _assignStudentToRouteUseCase = assignStudentToRouteUseCase;
        _getStudentStopOnRouteUseCase = getStudentStopOnRouteUseCase;
        _attachStopToRouteUseCase = attachStopToRouteUseCase;
        _assignBusToRouteUseCase = assignBusToRouteUseCase;
        _unassignBusFromRouteUseCase = unassignBusFromRouteUseCase;
        _createRouteScheduleUseCase = createRouteScheduleUseCase;
        _listRouteSchedulesUseCase = listRouteSchedulesUseCase;
    }

    [EnableRateLimiting("writes")]
    [HttpPost("{routeId:guid}/stops")]
    [RequirePermission("routes.manage")]
    public async Task<IActionResult> AttachStop(
        Guid routeId,
        [FromBody] AttachStopRequest request,
        CancellationToken ct)
    {
        var result = await _attachStopToRouteUseCase.ExecuteAsync(routeId, request.StopId,
            ct);
        return Ok(result);
    }

    [EnableRateLimiting("writes")]
    [HttpPost("{routeId:guid}/students")]
    [RequirePermission("routes.manage")]
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

    [EnableRateLimiting("writes")]
    [HttpPut("{routeId:guid}/bus")]
    [RequirePermission("routes.manage")]
    public async Task<IActionResult> AssignBus(
        Guid routeId,
        [FromBody] AssignBusRequest request,
        CancellationToken ct)
    {
        // Los conflictos se dejan subir: el middleware los traduce a 404/409
        // ProblemDetails (404 si la ruta no existe, 409 si el bus ya esta ocupado).
        var result = await _assignBusToRouteUseCase.ExecuteAsync(routeId, request.BusId, ct);
        return Ok(result);
    }

    [EnableRateLimiting("writes")]
    [HttpDelete("{routeId:guid}/bus")]
    [RequirePermission("routes.manage")]
    public async Task<IActionResult> UnassignBus(Guid routeId, CancellationToken ct)
    {
        await _unassignBusFromRouteUseCase.ExecuteAsync(routeId, ct);
        return NoContent();
    }

    [EnableRateLimiting("writes")]
    [HttpPost("{routeId:guid}/schedules")]
    [RequirePermission("routes.manage")]
    public async Task<IActionResult> CreateSchedule(
        Guid routeId,
        [FromBody] RouteScheduleRequestDto request,
        CancellationToken ct)
    {
        var result = await _createRouteScheduleUseCase.ExecuteAsync(routeId, request, ct);
        return Ok(result);
    }

    [HttpGet("{routeId:guid}/schedules")]
    public async Task<IActionResult> ListSchedules(Guid routeId, CancellationToken ct)
    {
        var result = await _listRouteSchedulesUseCase.ExecuteAsync(routeId, ct);
        return Ok(result);
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
}
