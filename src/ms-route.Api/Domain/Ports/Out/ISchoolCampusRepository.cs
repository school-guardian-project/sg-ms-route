namespace ms_route.Api.Domain.Ports.Out;

/// <summary>
/// Consulta de referencia a las sedes (School.SchoolCampus) de un colegio.
/// </summary>
public interface ISchoolCampusRepository
{
    Task<IReadOnlyList<Guid>> GetCampusIdsBySchoolAsync(Guid schoolId, CancellationToken ct = default);
}
