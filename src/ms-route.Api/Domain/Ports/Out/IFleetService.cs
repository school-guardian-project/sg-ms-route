namespace ms_route.Api.Domain.Ports.Out;

public interface IFleetService
{
    /// <summary>
    /// Bus activo asignado al conductor (perfil), o null si ms-fleet no
    /// devuelve ninguno. El bus es el eslabon conductor -> ruta.
    /// </summary>
    Task<Guid?> GetBusIdAssignedToDriverAsync(Guid profileId, CancellationToken ct = default);
}
