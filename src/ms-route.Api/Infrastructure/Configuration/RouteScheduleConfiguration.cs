using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ms_route.Api.Infrastructure.Persistence.Entity;

namespace ms_route.Api.Infrastructure.Configuration;

public class RouteScheduleConfiguration : IEntityTypeConfiguration<RouteScheduleEntity>
{
    public void Configure(EntityTypeBuilder<RouteScheduleEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RouteBusAssignmentsId).IsRequired();
        // La columna es VARCHAR(10) con CHECK de nombres de dia laborable: sin la
        // conversion EF escribiria el valor numerico del enum y violaria el CHECK.
        builder.Property(x => x.DayOfWeek).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.Direction).IsRequired().HasMaxLength(10);
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}
