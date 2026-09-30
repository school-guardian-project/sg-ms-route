using Microsoft.EntityFrameworkCore;
using ms_route.Api.Infrastructure.Configuration;
using ms_route.Api.Infrastructure.Persistence.Entity;

namespace ms_route.Api.Infrastructure.Persistence.Context;

public class RouteContext : DbContext
{
    public RouteContext(DbContextOptions options) : base(options) { }

    public DbSet<RouteEntity> Routes => Set<RouteEntity>();
    public DbSet<StopEntity> Stops => Set<StopEntity>();
    public DbSet<RouteStopEntity> RouteStops => Set<RouteStopEntity>();
    public DbSet<RouteBusAssignmentEntity> RouteBusAssignments => Set<RouteBusAssignmentEntity>();
    public DbSet<RouteStudentAssignmentEntity> RouteStudentAssignments => Set<RouteStudentAssignmentEntity>();
    public DbSet<RouteScheduleEntity> RouteSchedules => Set<RouteScheduleEntity>();
    public DbSet<RouteExecutionEntity> RouteExecutions => Set<RouteExecutionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("Route");

        modelBuilder.Entity<RouteEntity>().ToTable("Route", schema: "Route");
        modelBuilder.Entity<StopEntity>().ToTable("Stop", schema: "Route");
        modelBuilder.Entity<RouteStopEntity>().ToTable("RouteStop", schema: "Route");
        modelBuilder.Entity<RouteBusAssignmentEntity>().ToTable("RouteBusAssignments", schema: "Route");
        modelBuilder.Entity<RouteStudentAssignmentEntity>().ToTable("RouteStudentAssignments", schema: "Route");
        modelBuilder.Entity<RouteScheduleEntity>().ToTable("RouteSchedule", schema: "Route");
        modelBuilder.Entity<RouteExecutionEntity>().ToTable("RouteExecution", schema: "Route");

        modelBuilder.ApplyConfiguration(new RouteConfiguration());
        modelBuilder.ApplyConfiguration(new StopConfiguration());
        modelBuilder.ApplyConfiguration(new RouteStopConfiguration());
        modelBuilder.ApplyConfiguration(new RouteBusAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new RouteStudentAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new RouteScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new RouteExecutionConfiguration());
    }

    protected RouteContext() { }
}
