using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class InMemorySchoolCampusRepository : ISchoolCampusRepository
{
    public readonly Dictionary<Guid, List<Guid>> CampusesBySchool = new();

    public Task<IReadOnlyList<Guid>> GetCampusIdsBySchoolAsync(Guid schoolId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Guid>>(
            CampusesBySchool.TryGetValue(schoolId, out var ids) ? ids.ToList() : []);
}
