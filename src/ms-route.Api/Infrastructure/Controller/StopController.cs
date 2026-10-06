using Microsoft.AspNetCore.Mvc;
using ms_route.Api.Application.Dto;
using ms_route.Api.Domain.Ports.In;

namespace ms_route.Api.Infrastructure.Controller;

[ApiController]
[Route("api/stops")]
public class StopController : ControllerBase
{
    private readonly ICreateStopUseCase _createUseCase;
    private readonly IGetStopUseCase _getUseCase;
    private readonly IListStopUseCase _listUseCase;
    private readonly ISearchStopUseCase _searchUseCase;
    private readonly IUpdateStopUseCase _updateUseCase;
    private readonly IDeleteStopUseCase _deleteUseCase;

    public StopController(
        ICreateStopUseCase createUseCase,
        IGetStopUseCase getUseCase,
        IListStopUseCase listUseCase,
        ISearchStopUseCase searchUseCase,
        IUpdateStopUseCase updateUseCase,
        IDeleteStopUseCase deleteUseCase)
    {
        _createUseCase = createUseCase;
        _getUseCase = getUseCase;
        _listUseCase = listUseCase;
        _searchUseCase = searchUseCase;
        _updateUseCase = updateUseCase;
        _deleteUseCase = deleteUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StopRequestDto request, CancellationToken ct)
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

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search, CancellationToken ct)
    {
        var result = await _searchUseCase.ExecuteAsync(search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] StopRequestDto request, CancellationToken ct)
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
