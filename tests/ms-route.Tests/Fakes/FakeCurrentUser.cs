using ms_route.Api.Domain.Ports.Out;

namespace ms_route.Tests.Fakes;

public class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; set; } = true;
    public Guid? ProfileId { get; set; } = Guid.NewGuid();
    public int? RoleId { get; set; }
    public HashSet<string> Permissions { get; } = new();

    public bool HasPermission(string permission) => IsAuthenticated && Permissions.Contains(permission);
}
