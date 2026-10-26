using ms_route.Api.Application.Dto;

namespace ms_route.Api.Domain.Ports.In;

public interface IGetStudentRouteUseCase
{
    /// <summary>
    /// Ruta activa del estudiante, o <c>null</c> si todavia no tiene ruta asignada
    /// (el caso normal de un estudiante recien registrado). El controlador traduce
    /// <c>null</c> a 404; no es un error del servidor.
    /// </summary>
    /// <param name="studentId">ID del perfil del estudiante</param>
    /// <param name="isParent">Si es true, el solicitante es un acudiente/padre</param>
    /// <param name="studentProfileId">ID del perfil del estudiante (requerido si isParent=true)</param>
    /// <param name="ct">Token de cancelación</param>
    Task<RouteDetailDto?> ExecuteAsync(
        Guid studentId,
        bool isParent = false,
        Guid? studentProfileId = null,
        CancellationToken ct = default);
}
