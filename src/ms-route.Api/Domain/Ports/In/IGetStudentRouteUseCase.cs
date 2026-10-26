using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IGetStudentRouteUseCase
{
    Task<RouteDetailDto?> ExecuteAsync(Guid studentId, CancellationToken ct = default, bool isParent = false, Guid? studentProfileId = null);
}
