namespace ms_route.Api.Infrastructure.Persistence.Entity;

/// <summary>
/// Referencia de solo lectura a School.SchoolCampus (otro schema, mismo SQL Server).
/// </summary>
public class SchoolCampusRefEntity
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
}
