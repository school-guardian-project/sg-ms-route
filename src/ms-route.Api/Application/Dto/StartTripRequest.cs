namespace ms_route.Api.Application.Dto;

public class StartTripRequest
{
    public Guid RouteId { get; set; }
    public Guid BusId { get; set; }
    public Guid DriverId { get; set; }
}
