namespace ms_route.Api.Application.Dto;

public class StudentAssignmentDto
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public Guid StudentId { get; set; }
    public Guid StopId { get; set; }
    public string Status { get; set; } = string.Empty;
}
