using ms_route.Api.Domain.Model;

namespace ms_route.Api.Domain.Ports.Out;

public interface IFleetService
{
    /// <summary>
    /// Bus activo asignado al conductor (perfil), o null si ms-fleet no
    /// devuelve ninguno. El bus es el eslabon conductor -> ruta.
    /// </summary>
    Task<BusInfo?> GetBusAssignedToDriverAsync(Guid profileId, CancellationToken ct = default);

    /// <summary>
    /// Detalle del bus (placa, capacidad y conductor asignado) o null si
    /// ms-fleet no lo conoce. Enriquece la vista de ruta del acudiente (FR-9.1).
    /// </summary>
    Task<BusInfo?> GetBusDetailsAsync(Guid busId, CancellationToken ct = default);
}
