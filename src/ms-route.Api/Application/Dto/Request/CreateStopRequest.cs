using System.ComponentModel.DataAnnotations;

namespace ms_route.Api.Application.Dto.Request;

/// <summary>
/// Request DTO for creating a new stop.
/// </summary>
public sealed class CreateStopRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public Guid CityId { get; init; }

    [Required]
    public Guid SchoolId { get; init; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Address { get; init; } = string.Empty;

    [Required]
    [Range(-180, 180)]
    public decimal Longitude { get; init; }

    [Required]
    [Range(-90, 90)]
    public decimal Latitude { get; init; }

    // Opcional: ruta a la que pasa la parada al crear.
    public Guid? RouteId { get; init; }

    // Ruta que se reemplaza. Una parada puede estar en varias rutas, asi que
    // sin este dato solo se mueve si tiene una sola; si no, se agrega.
    public Guid? PreviousRouteId { get; init; }
}