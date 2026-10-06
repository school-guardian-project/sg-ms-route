using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IUpdateStopUseCase
{
    Task ExecuteAsync(Guid id, StopRequestDto request, CancellationToken ct = default);
}
