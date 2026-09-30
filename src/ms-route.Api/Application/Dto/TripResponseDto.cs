namespace ms_route.Api.Application.Dto;

public class TripResponseDto
{
    public Guid Id { get; set; }
    public Guid BusId { get; set; }
    public Guid DriverId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
