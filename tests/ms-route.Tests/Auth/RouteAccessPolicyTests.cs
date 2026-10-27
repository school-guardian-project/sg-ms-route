using ms_route.Api.Application.Security;
using ms_route.Api.Domain.Exceptions;
using ms_route.Api.Domain.Model;
using ms_route.Tests.Fakes;
using NUnit.Framework;

namespace ms_route.Tests.Auth;

public class RouteAccessPolicyTests
{
    private const int Admin = 1;
    private const int Student = 2;
    private const int Driver = 3;
    private const int Parent = 4;
    private const int SuperAdmin = 5;

    private FakeCurrentUser _user = null!;
    private InMemoryRouteExecutionRepository _executions = null!;
    private FakeStudentGuardianDirectory _guardians = null!;
    private RouteAccessPolicy _policy = null!;

    [SetUp]
    public void SetUp()
    {
        _user = new FakeCurrentUser();
        _executions = new InMemoryRouteExecutionRepository();
        _guardians = new FakeStudentGuardianDirectory();
        _policy = new RouteAccessPolicy(_user, _executions, _guardians);
    }

    // --- Conductor (C2): su id sale del token ---

    [Test]
    public void Conductor_SinDriverId_UsaElDelToken()
    {
        _user.RoleId = Driver;

        Assert.That(_policy.ResolveDriverId(null), Is.EqualTo(_user.ProfileId));
    }

    [Test]
    public void Conductor_ConSuPropioDriverId_LoAcepta()
    {
        _user.RoleId = Driver;

        Assert.That(_policy.ResolveDriverId(_user.ProfileId), Is.EqualTo(_user.ProfileId));
    }

    [Test]
    public void Conductor_ConDriverIdDeOtro_Devuelve403()
    {
        _user.RoleId = Driver;

        Assert.Throws<AccessDeniedException>(() => _policy.ResolveDriverId(Guid.NewGuid()));
    }

    [Test]
    public void Staff_ConsultaElConductorQueIndica()
    {
        _user.RoleId = Admin;
        _user.Permissions.Add("routes.read");
        var driverId = Guid.NewGuid();

        Assert.That(_policy.ResolveDriverId(driverId), Is.EqualTo(driverId));
    }

    [Test]
    public void Staff_SinDriverId_Devuelve400()
    {
        _user.RoleId = SuperAdmin;
        _user.Permissions.Add("routes.read");

        Assert.Throws<ArgumentException>(() => _policy.ResolveDriverId(null));
    }

    [Test]
    public void Acudiente_ConRoutesRead_NoConsultaConductores()
    {
        // Parent tiene routes.read, pero no es staff.
        _user.RoleId = Parent;
        _user.Permissions.Add("routes.read");

        Assert.Throws<AccessDeniedException>(() => _policy.ResolveDriverId(Guid.NewGuid()));
    }

    [Test]
    public void SinAutenticar_Devuelve403()
    {
        _user.IsAuthenticated = false;
        _user.RoleId = Driver;

        Assert.Throws<AccessDeniedException>(() => _policy.ResolveDriverId(null));
    }

    [Test]
    public void TerminarViaje_PropioConductor_Permitido()
    {
        _user.RoleId = Driver;
        var trip = AddTrip(_user.ProfileId!.Value);

        Assert.DoesNotThrowAsync(() => _policy.EnsureDriverOwnsTripAsync(trip.Id));
    }

    [Test]
    public void TerminarViaje_DeOtroConductor_Devuelve403()
    {
        _user.RoleId = Driver;
        var trip = AddTrip(Guid.NewGuid());

        Assert.ThrowsAsync<AccessDeniedException>(() => _policy.EnsureDriverOwnsTripAsync(trip.Id));
    }

    [Test]
    public void TerminarViaje_Inexistente_Devuelve404()
    {
        _user.RoleId = Driver;

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _policy.EnsureDriverOwnsTripAsync(Guid.NewGuid()));
        Assert.That(ex!.Message, Does.Contain("not found"));
    }

    [Test]
    public void TerminarViaje_StaffNoPuede()
    {
        _user.RoleId = Admin;
        _user.Permissions.Add("routes.read");
        var trip = AddTrip(Guid.NewGuid());

        Assert.ThrowsAsync<AccessDeniedException>(() => _policy.EnsureDriverOwnsTripAsync(trip.Id));
    }

    // --- Ruta de un estudiante ---

    [Test]
    public void Estudiante_VeSuPropiaRuta()
    {
        _user.RoleId = Student;

        Assert.DoesNotThrowAsync(() => _policy.EnsureCanReadStudentRouteAsync(_user.ProfileId!.Value));
    }

    [Test]
    public void Estudiante_NoVeLaRutaDeOtro()
    {
        _user.RoleId = Student;

        Assert.ThrowsAsync<AccessDeniedException>(() => _policy.EnsureCanReadStudentRouteAsync(Guid.NewGuid()));
    }

    [Test]
    public void Acudiente_VeLaRutaDeSuHijo()
    {
        _user.RoleId = Parent;
        var child = Guid.NewGuid();
        _guardians.GuardiansByStudent[child] = [_user.ProfileId!.Value];

        Assert.DoesNotThrowAsync(() => _policy.EnsureCanReadStudentRouteAsync(child));
    }

    [Test]
    public void Acudiente_NoVeLaRutaDeUnEstudianteAjeno()
    {
        _user.RoleId = Parent;
        var other = Guid.NewGuid();
        _guardians.GuardiansByStudent[other] = [Guid.NewGuid()];

        Assert.ThrowsAsync<AccessDeniedException>(() => _policy.EnsureCanReadStudentRouteAsync(other));
    }

    [Test]
    public void Staff_VeLaRutaDeCualquierEstudiante()
    {
        _user.RoleId = Admin;
        _user.Permissions.Add("routes.read");

        Assert.DoesNotThrowAsync(() => _policy.EnsureCanReadStudentRouteAsync(Guid.NewGuid()));
    }

    [Test]
    public void Conductor_NoVeRutasDeEstudiantes()
    {
        _user.RoleId = Driver;
        _user.Permissions.Add("routes.read");

        Assert.ThrowsAsync<AccessDeniedException>(() => _policy.EnsureCanReadStudentRouteAsync(Guid.NewGuid()));
    }

    private RouteExecution AddTrip(Guid driverId)
    {
        var trip = new RouteExecution { Id = Guid.NewGuid(), DriverId = driverId, RouteId = Guid.NewGuid(), Status = Status.Active };
        _executions.Executions.Add(trip);
        return trip;
    }
}
