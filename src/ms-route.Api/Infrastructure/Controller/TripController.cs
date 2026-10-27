using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;

namespace ms_route.Api.Infrastructure.Controller;

[ApiController]
[Route("api/trips")]
public class TripController : ControllerBase
{
    private readonly IStartTripUseCase _startTripUseCase;
    private readonly IEndTripUseCase _endTripUseCase;
    private readonly IGetCurrentTripUseCase _getCurrentTripUseCase;
    private readonly IGetTripStatusUseCase _getTripStatusUseCase;

    public TripController(
        IStartTripUseCase startTripUseCase,
        IEndTripUseCase endTripUseCase,
        IGetCurrentTripUseCase getCurrentTripUseCase,
        IGetTripStatusUseCase getTripStatusUseCase)
    {
        _startTripUseCase = startTripUseCase;
        _endTripUseCase = endTripUseCase;
        _getCurrentTripUseCase = getCurrentTripUseCase;
        _getTripStatusUseCase = getTripStatusUseCase;
    }

    [EnableRateLimiting("writes")]
    [HttpPost("start")]
    public async Task<IActionResult> StartTrip([FromBody] StartTripRequest request, CancellationToken ct)
    {
        var result = await _startTripUseCase.ExecuteAsync(request.RouteId, request.BusId, request.DriverId, ct);
        return Ok(result);
    }

    [EnableRateLimiting("writes")]
    [HttpPost("end")]
    public async Task<IActionResult> EndTrip(Guid tripId, CancellationToken ct)
    {
        var result = await _endTripUseCase.ExecuteAsync(tripId, ct);
        return Ok(result);
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentTrip(Guid driverId, CancellationToken ct)
    {
        var result = await _getCurrentTripUseCase.ExecuteAsync(driverId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetTripStatus(Guid driverId, CancellationToken ct)
    {
        var result = await _getTripStatusUseCase.ExecuteAsync(driverId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}