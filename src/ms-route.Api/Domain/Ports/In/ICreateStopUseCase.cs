using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface ICreateStopUseCase
{
    Task<StopResponseDto> ExecuteAsync(StopRequestDto request, CancellationToken ct = default);
}
