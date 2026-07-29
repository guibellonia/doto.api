using System.Linq.Expressions;
using Doto.Domain.Common;
using Doto.Domain.Entities;
using Doto.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace Doto.Infrastructure.Persistence;

public class DotoDbContext(DbContextOptions<DotoDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    public DbSet<HealthCondition> HealthConditions => Set<HealthCondition>();

    public DbSet<ChildInvite> ChildInvites => Set<ChildInvite>();

    public DbSet<Medication> Medications => Set<Medication>();

    public DbSet<MedicationSchedule> MedicationSchedules => Set<MedicationSchedule>();

    public DbSet<ScheduleTimeSlot> ScheduleTimeSlots => Set<ScheduleTimeSlot>();

    public DbSet<DoseOccurrence> DoseOccurrences => Set<DoseOccurrence>();

    public DbSet<ScheduleAdjustment> ScheduleAdjustments => Set<ScheduleAdjustment>();

    public DbSet<SymptomRecord> SymptomRecords => Set<SymptomRecord>();

    public DbSet<VitalReading> VitalReadings => Set<VitalReading>();

    public DbSet<Device> Devices => Set<Device>();

    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();

    public DbSet<ReportExport> ReportExports => Set<ReportExport>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DotoDbContext).Assembly);

        ApplyGeneratedIdDefaults(modelBuilder);
        ApplySoftDeleteQueryFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplyGeneratedIdDefaults(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType == typeof(AppUser) || !typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(EntityBase.Id))
                .HasDefaultValueSql("gen_random_uuid()");
        }
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var deletedAt = Expression.Property(parameter, nameof(ISoftDeletable.DeletedAt));
            var filter = Expression.Lambda(
                Expression.Equal(deletedAt, Expression.Constant(null, typeof(DateTime?))),
                parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}
