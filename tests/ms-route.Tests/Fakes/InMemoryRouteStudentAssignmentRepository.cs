using ms_route.Api.Domain.Model;
using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class InMemoryRouteStudentAssignmentRepository : IRouteStudentAssignmentRepository
{
    public readonly List<RouteStudentAssignment> Assignments = new();

    public Task<RouteStudentAssignment> SaveAsync(RouteStudentAssignment assignment, CancellationToken ct = default)
    {
        Assignments.Add(assignment);
        return Task.FromResult(assignment);
    }

    public Task<RouteStudentAssignment> UpdateAsync(RouteStudentAssignment assignment, CancellationToken ct = default)
    {
        var index = Assignments.FindIndex(a => a.Id == assignment.Id);
        if (index >= 0)
            Assignments[index] = assignment;

        return Task.FromResult(assignment);
    }

    public Task<RouteStudentAssignment?> GetActiveByProfileIdAsync(Guid profileId, CancellationToken ct = default)
        => Task.FromResult(Assignments.FirstOrDefault(a => a.ProfileId == profileId && a.Status == Status.Active));
}
