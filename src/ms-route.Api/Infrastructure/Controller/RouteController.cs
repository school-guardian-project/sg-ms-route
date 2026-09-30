using Microsoft.AspNetCore.Mvc;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;

namespace ms_route.Api.Infrastructure.Controller;

[ApiController]
[Route("api/routes")]
public class RouteController : ControllerBase
{
    private readonly ICreateRouteUseCase _createUseCase;
    private readonly IGetRouteUseCase _getUseCase;
    private readonly IListRouteUseCase _listUseCase;
    private readonly IUpdateRouteUseCase _updateUseCase;
    private readonly IDeleteRouteUseCase _deleteUseCase;

    public RouteController(
        ICreateRouteUseCase createUseCase,
        IGetRouteUseCase getUseCase,
        IListRouteUseCase listUseCase,
        IUpdateRouteUseCase updateUseCase,
        IDeleteRouteUseCase deleteUseCase)
    {
        _createUseCase = createUseCase;
        _getUseCase = getUseCase;
        _listUseCase = listUseCase;
        _updateUseCase = updateUseCase;
        _deleteUseCase = deleteUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RouteRequestDto request, CancellationToken ct)
    {
        var result = await _createUseCase.ExecuteAsync(request, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _listUseCase.ExecuteAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RouteRequestDto request, CancellationToken ct)
    {
        await _updateUseCase.ExecuteAsync(id, request, ct);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _deleteUseCase.ExecuteAsync(id, ct);
        return NoContent();
    }
}
