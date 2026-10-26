using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface ICreateRouteScheduleUseCase
{
    Task<List<ScheduleDetailDto>> ExecuteAsync(
        Guid routeId,
        RouteRequestDto request,
        CancellationToken ct = default);
}
