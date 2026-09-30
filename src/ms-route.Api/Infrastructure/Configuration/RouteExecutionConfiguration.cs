using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ms_route.Api.Infrastructure.Persistence.Entity;

namespace ms_route.Api.Infrastructure.Configuration;

public class RouteExecutionConfiguration : IEntityTypeConfiguration<RouteExecutionEntity>
{
    public void Configure(EntityTypeBuilder<RouteExecutionEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BusId).IsRequired();
        builder.Property(x => x.DriverId).IsRequired();
        builder.Property(x => x.StartDateTime).IsRequired();
        builder.Property(x => x.EndDateTime);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}
