using Microsoft.AspNetCore.Mvc;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Api.Infrastructure.Controller;

[ApiController]
[Route("api/stops")]
public class StopController : ControllerBase
{
    private readonly IStopRepository _repository;

    public StopController(IStopRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StopRequestDto request, CancellationToken ct)
    {
        var stop = new Stop
        {
            Id = Guid.NewGuid(),
            CityId = request.CityId,
            SchoolId = request.SchoolId,
            Address = request.Address,
            Longitude = request.Longitude,
            Latitude = request.Latitude,
            Status = StopStatus.Active
        };

        var saved = await _repository.SaveAsync(stop, ct);

        return Ok(new StopResponseDto
        {
            Id = saved.Id,
            Address = saved.Address,
            Longitude = saved.Longitude,
            Latitude = saved.Latitude,
            Status = saved.Status.ToString(),
            CityId = saved.CityId,
            SchoolId = saved.SchoolId,
            CreatedAt = saved.CreatedAt,
            UpdatedAt = saved.UpdatedAt
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var stops = await _repository.GetAllAsync(ct);

        return Ok(stops.Select(s => new StopListDto
        {
            Id = s.Id,
            Address = s.Address,
            Longitude = s.Longitude,
            Latitude = s.Latitude,
            Status = s.Status.ToString()
        }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var stop = await _repository.GetByIdAsync(id, ct);
        if (stop is null)
            return NotFound();

        return Ok(new StopResponseDto
        {
            Id = stop.Id,
            Address = stop.Address,
            Longitude = stop.Longitude,
            Latitude = stop.Latitude,
            Status = stop.Status.ToString(),
            CityId = stop.CityId,
            SchoolId = stop.SchoolId,
            CreatedAt = stop.CreatedAt,
            UpdatedAt = stop.UpdatedAt
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] StopRequestDto request, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing is null)
            return NotFound();

        existing.CityId = request.CityId;
        existing.SchoolId = request.SchoolId;
        existing.Address = request.Address;
        existing.Longitude = request.Longitude;
        existing.Latitude = request.Latitude;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existing, ct);

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _repository.DeleteAsync(id, ct);
        return NoContent();
    }
}
