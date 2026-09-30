using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ms_route.Api.Infrastructure.Persistence.Entity;

namespace ms_route.Api.Infrastructure.Configuration;

public class RouteConfiguration : IEntityTypeConfiguration<RouteEntity>
{
    public void Configure(EntityTypeBuilder<RouteEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CampuseId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(30);
        builder.Property(x => x.TargetSector).IsRequired().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
