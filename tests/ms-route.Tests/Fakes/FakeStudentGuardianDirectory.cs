using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class FakeStudentGuardianDirectory : IStudentGuardianDirectory
{
    public Dictionary<Guid, List<Guid>> GuardiansByStudent { get; } = new();

    public Task<IReadOnlyCollection<Guid>> GetGuardianProfileIdsAsync(
        Guid studentProfileId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyCollection<Guid>>(
            GuardiansByStudent.TryGetValue(studentProfileId, out var guardians) ? guardians : []);
}
