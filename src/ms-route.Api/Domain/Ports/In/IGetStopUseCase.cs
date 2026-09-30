using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IGetStopUseCase
{
    Task<StopResponseDto> ExecuteAsync(Guid id, CancellationToken ct = default);
}
