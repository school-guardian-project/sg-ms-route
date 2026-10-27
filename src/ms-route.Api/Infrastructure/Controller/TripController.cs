using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ms_route.Api.Application.Dto.Request;
using ms_route.Api.Application.Security;
using ms_route.Api.Domain.Ports.In;
using ms_route.Api.Infrastructure.Auth;

namespace ms_route.Api.Infrastructure.Controller;

[ApiController]
[Route("api/trips")]
public class TripController : ControllerBase
{
    private readonly IStartTripUseCase _startTripUseCase;
    private readonly IEndTripUseCase _endTripUseCase;
    private readonly IGetCurrentTripUseCase _getCurrentTripUseCase;
    private readonly IGetTripStatusUseCase _getTripStatusUseCase;
    private readonly IRouteAccessPolicy _access;

    public TripController(
        IStartTripUseCase startTripUseCase,
        IEndTripUseCase endTripUseCase,
        IGetCurrentTripUseCase getCurrentTripUseCase,
        IGetTripStatusUseCase getTripStatusUseCase,
        IRouteAccessPolicy access)
    {
        _startTripUseCase = startTripUseCase;
        _endTripUseCase = endTripUseCase;
        _getCurrentTripUseCase = getCurrentTripUseCase;
        _getTripStatusUseCase = getTripStatusUseCase;
        _access = access;
    }

    [EnableRateLimiting("writes")]
    [HttpPost("start")]
    [RequireRole(RequireRoleAttribute.DriverRoleId)]
    public async Task<IActionResult> StartTrip([FromBody] StartTripRequest request, CancellationToken ct)
    {
        var driverId = _access.ResolveDriverId(request.DriverId);
        var result = await _startTripUseCase.ExecuteAsync(request.RouteId, request.BusId, driverId, ct);
        return Ok(result);
    }

    [EnableRateLimiting("writes")]
    [HttpPost("end")]
    [RequireRole(RequireRoleAttribute.DriverRoleId)]
    public async Task<IActionResult> EndTrip([FromQuery] Guid tripId, CancellationToken ct)
    {
        await _access.EnsureDriverOwnsTripAsync(tripId, ct);
        var result = await _endTripUseCase.ExecuteAsync(tripId, ct);
        return Ok(result);
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentTrip([FromQuery] Guid? driverId, CancellationToken ct)
    {
        var result = await _getCurrentTripUseCase.ExecuteAsync(_access.ResolveDriverId(driverId), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetTripStatus([FromQuery] Guid? driverId, CancellationToken ct)
    {
        var result = await _getTripStatusUseCase.ExecuteAsync(_access.ResolveDriverId(driverId), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
