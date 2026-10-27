using ms_route.Api.Application.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace ms_route.Api.Domain.Ports.In;

public interface IGetTripStatusUseCase
{
    Task<GetTripStatusResponseDto?> ExecuteAsync(Guid driverId, CancellationToken ct = default);
}