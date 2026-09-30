using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IEndTripUseCase
{
    Task<TripResponseDto> ExecuteAsync(Guid tripId, CancellationToken ct = default);
}
