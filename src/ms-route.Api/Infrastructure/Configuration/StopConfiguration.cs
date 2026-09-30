using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ms_route.Api.Infrastructure.Persistence.Entity;

namespace ms_route.Api.Infrastructure.Configuration;

public class StopConfiguration : IEntityTypeConfiguration<StopEntity>
{
    public void Configure(EntityTypeBuilder<StopEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CityId).IsRequired();
        builder.Property(x => x.SchoolId).IsRequired();
        builder.Property(x => x.Address).IsRequired().HasMaxLength(30);
        builder.Property(x => x.Longitude).IsRequired().HasPrecision(12, 2);
        builder.Property(x => x.Latitude).IsRequired().HasPrecision(12, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}
