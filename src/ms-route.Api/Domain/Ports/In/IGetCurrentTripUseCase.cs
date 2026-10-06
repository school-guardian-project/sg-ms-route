using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IGetCurrentTripUseCase
{
    Task<TripResponseDto?> ExecuteAsync(Guid driverId, CancellationToken ct = default);
}
