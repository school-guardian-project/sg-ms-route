using Microsoft.AspNetCore.Mvc;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;

namespace ms_route.Api.Infrastructure.Controller;

[ApiController]
[Route("api/routes")]
public class RouteDetailController : ControllerBase
{
    private readonly IGetCurrentRouteUseCase _getCurrentRouteUseCase;
    private readonly IGetStudentRouteUseCase _getStudentRouteUseCase;
    private readonly IGetDriverRouteTodayUseCase _getDriverRouteTodayUseCase;

    public RouteDetailController(
        IGetCurrentRouteUseCase getCurrentRouteUseCase,
        IGetStudentRouteUseCase getStudentRouteUseCase,
        IGetDriverRouteTodayUseCase getDriverRouteTodayUseCase)
    {
        _getCurrentRouteUseCase = getCurrentRouteUseCase;
        _getStudentRouteUseCase = getStudentRouteUseCase;
        _getDriverRouteTodayUseCase = getDriverRouteTodayUseCase;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentRoute(Guid driverId, CancellationToken ct)
    {
        var result = await _getCurrentRouteUseCase.ExecuteAsync(driverId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Ruta del conductor valida segun el horario: siempre responde 200 con un
    /// estado (ACTIVE/SCHEDULED/NO_ROUTE/NO_SCHEDULE) para que el cliente decida
    /// si mostrar el recorrido o solo el recordatorio.
    /// </summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayRoute(Guid driverId, CancellationToken ct)
    {
        var result = await _getDriverRouteTodayUseCase.ExecuteAsync(driverId, ct);
        return Ok(result);
    }

    [HttpGet("student/{studentId:guid}")]
    public async Task<IActionResult> GetStudentRoute(Guid studentId, CancellationToken ct)
    {
        var result = await _getStudentRouteUseCase.ExecuteAsync(studentId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
