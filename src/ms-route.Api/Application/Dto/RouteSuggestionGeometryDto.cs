namespace ms_route.Api.Application.Dto;

public class RouteSuggestionGeometryDto
{
    /// <summary>Puntos del trazado vial en orden de transito.</summary>
    public List<GeoPointDto> Points { get; set; } = new();

    public double DistanceMeters { get; set; }
    public double DurationSeconds { get; set; }
}
