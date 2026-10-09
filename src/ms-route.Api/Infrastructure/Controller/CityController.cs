using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ms_route.Api.Application.Dto;
using ms_route.Api.Infrastructure.Persistence.Entity;
using RouteContext = ms_route.Api.Infrastructure.Persistence.Context.RouteContext;

namespace ms_route.Api.Infrastructure.Controller;

[ApiController]
[Route("api/cities")]
public class CityController : ControllerBase
{
    private readonly RouteContext _context;

    public CityController(RouteContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CityListDto>>> List(CancellationToken ct)
    {
        var cities = await _context.Cities
            .AsNoTracking()
            .Where(city => city.Status == "Active")
            .OrderBy(city => city.Name)
            .Select(city => new CityListDto
            {
                Id = city.Id,
                Name = city.Name
            })
            .ToListAsync(ct);

        return Ok(cities);
    }
}
