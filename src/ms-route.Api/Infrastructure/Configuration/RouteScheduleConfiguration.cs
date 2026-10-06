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
        builder.Property(x => x.DayOfWeek).IsRequired();
        builder.Property(x => x.Direction).IsRequired().HasMaxLength(10);
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}
