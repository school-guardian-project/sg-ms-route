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

    public RouteDetailController(
        IGetCurrentRouteUseCase getCurrentRouteUseCase,
        IGetStudentRouteUseCase getStudentRouteUseCase)
    {
        _getCurrentRouteUseCase = getCurrentRouteUseCase;
        _getStudentRouteUseCase = getStudentRouteUseCase;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentRoute(Guid driverId, CancellationToken ct)
    {
        var result = await _getCurrentRouteUseCase.ExecuteAsync(driverId, ct);
        return Ok(result);
    }

    [HttpGet("student/{studentId:guid}")]
    public async Task<IActionResult> GetStudentRoute(Guid studentId, CancellationToken ct)
    {
        var result = await _getStudentRouteUseCase.ExecuteAsync(studentId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
