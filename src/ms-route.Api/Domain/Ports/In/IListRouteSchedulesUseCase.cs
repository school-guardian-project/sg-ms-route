using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IListRouteSchedulesUseCase
{
    Task<IReadOnlyList<ScheduleDetailDto>> ExecuteAsync(Guid routeId, CancellationToken ct = default);
}
