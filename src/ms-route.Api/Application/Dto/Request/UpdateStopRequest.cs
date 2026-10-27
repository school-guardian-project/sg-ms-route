using System.ComponentModel.DataAnnotations;

namespace ms_route.Api.Application.Dto.Request;

/// <summary>
/// Request DTO for updating an existing stop.
/// All properties are optional - only provided values will be updated.
/// </summary>
public sealed class UpdateStopRequest
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }

    public Guid? CityId { get; init; }

    public Guid? SchoolId { get; init; }

    [StringLength(200, MinimumLength = 1)]
    public string? Address { get; init; }

    [Range(-180, 180)]
    public decimal? Longitude { get; init; }

    [Range(-90, 90)]
    public decimal? Latitude { get; init; }

    // Opcional: ruta a la que pasa la parada al actualizar.
    public Guid? RouteId { get; init; }

    // Ruta que se reemplaza.
    public Guid? PreviousRouteId { get; init; }
}