using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IAssignBusToRouteUseCase
{
    Task<BusAssignmentDto> ExecuteAsync(Guid routeId, Guid busId, CancellationToken ct = default);
}
