using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IAssignStudentToRouteUseCase
{
    Task<StudentAssignmentDto> ExecuteAsync(Guid routeId, Guid studentId, Guid stopId, CancellationToken ct = default);
}
