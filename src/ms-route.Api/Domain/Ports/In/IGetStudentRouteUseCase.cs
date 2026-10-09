using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IGetStudentRouteUseCase
{
    /// <summary>
    /// Ruta activa del estudiante, o <c>null</c> si todavia no tiene ruta asignada
    /// (el caso normal de un estudiante recien registrado). El controlador traduce
    /// <c>null</c> a 404; no es un error del servidor.
    /// </summary>
    Task<RouteDetailDto?> ExecuteAsync(Guid studentId, CancellationToken ct = default);
}
