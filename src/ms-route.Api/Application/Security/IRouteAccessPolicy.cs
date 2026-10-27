namespace ms_route.Api.Application.Security;

/// <summary>
/// Reglas de acceso de los endpoints REST que actuan en nombre de un conductor
/// o de un estudiante. Vive fuera de los casos de uso porque el gRPC interno
/// los reutiliza sin usuario autenticado.
/// </summary>
public interface IRouteAccessPolicy
{
    /// <summary>
    /// Conductor a consultar o a usar en el viaje: el conductor autenticado
    /// siempre es el mismo (un id distinto es 403); el staff con routes.read
    /// consulta el conductor que indique.
    /// </summary>
    Guid ResolveDriverId(Guid? requestedDriverId);

    /// <summary>Solo el conductor que inicio el viaje puede terminarlo.</summary>
    Task EnsureDriverOwnsTripAsync(Guid tripId, CancellationToken ct = default);

    /// <summary>
    /// La ruta de un estudiante la ven el propio estudiante, sus acudientes y
    /// el staff con routes.read.
    /// </summary>
    Task EnsureCanReadStudentRouteAsync(Guid studentProfileId, CancellationToken ct = default);
}
