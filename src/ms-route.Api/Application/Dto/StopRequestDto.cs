namespace ms_route.Api.Application.Dto;

public class StopRequestDto
{
    public string Name { get; set; } = string.Empty;
    public Guid CityId { get; set; }
    public Guid SchoolId { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal Longitude { get; set; }
    public decimal Latitude { get; set; }

    // Opcional: ruta a la que pasa la parada al actualizar.
    public Guid? RouteId { get; set; }

    // Ruta que se reemplaza. Una parada puede estar en varias rutas, asi que
    // sin este dato solo se mueve si tiene una sola; si no, se agrega.
    public Guid? PreviousRouteId { get; set; }
}
