using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IStartTripUseCase
{
    Task<TripResponseDto> ExecuteAsync(Guid routeId, Guid busId, Guid driverId, CancellationToken ct = default);
}
