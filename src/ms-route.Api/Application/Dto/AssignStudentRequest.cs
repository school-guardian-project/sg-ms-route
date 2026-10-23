namespace ms_route.Api.Application.Dto;

public class AssignStudentRequest
{
    public Guid StudentId { get; set; }
    public Guid StopId { get; set; }
}
