using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ms_route.Api.Infrastructure.Persistence.Entity;

namespace ms_route.Api.Infrastructure.Configuration;

public class RouteStopConfiguration : IEntityTypeConfiguration<RouteStopEntity>
{
    public void Configure(EntityTypeBuilder<RouteStopEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RouteId).IsRequired();
        builder.Property(x => x.StopId).IsRequired();
        builder.Property(x => x.OrderSequence).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}
