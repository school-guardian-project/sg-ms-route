using Microsoft.AspNetCore.Mvc;
using ms_route.Api.Domain.Ports.In;

namespace ms_route.Api.Infrastructure.Controller;

[ApiController]
[Route("api/cities")]
public class CityController : ControllerBase
{
    private readonly IListCityUseCase _listUseCase;

    public CityController(IListCityUseCase listUseCase)
    {
        _listUseCase = listUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _listUseCase.ExecuteAsync(ct);
        return Ok(result);
    }
}
