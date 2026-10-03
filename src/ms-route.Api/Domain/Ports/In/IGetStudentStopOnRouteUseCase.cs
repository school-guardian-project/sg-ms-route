using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IGetStudentStopOnRouteUseCase
{
    Task<StudentRouteStopDto?> ExecuteAsync(Guid routeId, Guid studentProfileId, CancellationToken ct = default);
}
