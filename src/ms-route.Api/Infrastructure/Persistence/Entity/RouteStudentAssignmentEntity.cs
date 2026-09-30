using ms_route.Api.Domain.Model;

namespace ms_route.Api.Infrastructure.Persistence.Entity;

public class RouteStudentAssignmentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public Guid RouteStopId { get; set; }
    public Status Status { get; set; } = Status.Active;
}
