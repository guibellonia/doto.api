using Microsoft.EntityFrameworkCore;
using Doto.Domain.Entities;

namespace Doto.Infrastructure.Persistence;

public class DotoDbContext : DbContext
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Medicine> Medicines => Set<Medicine>();

    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<ScheduleTime> ScheduleTimes => Set<ScheduleTime>();
    public DbSet<ScheduleWeekDay> ScheduleWeekDays => Set<ScheduleWeekDay>();

    public DbSet<MedicineDoseOccurrence> MedicineDoseOccurrences => Set<MedicineDoseOccurrence>();
    public DbSet<VitalSignRecord> VitalSignRecords => Set<VitalSignRecord>();
    public DbSet<SymptomRecord> SymptomRecords => Set<SymptomRecord>();

    public DotoDbContext(DbContextOptions<DotoDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        // -------- Person --------
        modelBuilder.Entity<Person>(builder =>
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(120);

            builder.Property(p => p.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(p => p.SupabaseUserId)
                   .HasMaxLength(50);

            builder.Property(p => p.CreatedAt)
                   .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                   .IsRequired();

            builder.Property(p => p.UpdatedAt)
                   .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                   .IsRequired();

            builder.Property(p => p.BirthDate)
                   .HasConversion(
                        v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : (DateTime?)null,
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);
        });

        // -------- Medicine --------
        modelBuilder.Entity<Medicine>(builder =>
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(m => m.DosageValue)
                   .IsRequired();

            builder.Property(m => m.DosageUnit)
                   .HasConversion<int>() // enum -> int
                   .IsRequired();

            // DateOnly -> DateTime (UTC)
            builder.Property(m => m.StartDate)
                   .HasConversion(
                        v => DateTime.SpecifyKind(v.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                        v => DateOnly.FromDateTime(v.ToUniversalTime()))
                   .IsRequired();

            builder.Property(m => m.EndDate)
                   .HasConversion(
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc) : (DateTime?)null,
                        v => v.HasValue ? DateOnly.FromDateTime(v.Value.ToUniversalTime()) : (DateOnly?)null);

            builder.Property(m => m.CreatedAt)
                   .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                   .IsRequired();

            builder.HasOne<Person>()
                   .WithMany()
                   .HasForeignKey(m => m.PersonId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // -------- Schedule --------
        modelBuilder.Entity<Schedule>(builder =>
        {
            builder.ToTable("Schedules");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.ScheduleType)
                   .HasConversion<int>() // MedicineScheduleType enum
                   .IsRequired();

            // TimeOnly? -> TimeSpan?
            builder.Property(s => s.TimeOfDay)
                   .HasConversion(
                        v => v.HasValue ? v.Value.ToTimeSpan() : (TimeSpan?)null,
                        v => v.HasValue ? TimeOnly.FromTimeSpan(v.Value) : (TimeOnly?)null
                    );

            builder.Property(s => s.IntervalInHours);

            builder.Property(s => s.FirstDoseAt);

            builder.Property(s => s.PreAlarmMinutes);
            builder.Property(s => s.PosAlarmMinutes);

            builder.Property(s => s.IsActive)
                   .HasDefaultValue(true);

            builder.Property(s => s.CreatedAt)
                   .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                   .IsRequired();

            builder.HasOne(s => s.Medicine)
                   .WithMany() // se você criar ICollection<Schedule> em Medicine, pode pôr .WithMany(m => m.Schedules)
                   .HasForeignKey(s => s.MedicineId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.TimesOfDay)
                   .WithOne(t => t.Schedule)
                   .HasForeignKey(t => t.ScheduleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.WeekDays)
                   .WithOne(w => w.Schedule)
                   .HasForeignKey(w => w.ScheduleId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // -------- ScheduleTime --------
        modelBuilder.Entity<ScheduleTime>(builder =>
        {
            builder.ToTable("ScheduleTimes");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Time)
                   .HasConversion(
                        v => v.ToTimeSpan(),
                        v => TimeOnly.FromTimeSpan(v))
                   .IsRequired();

            builder.HasOne(t => t.Schedule)
                   .WithMany(s => s.TimesOfDay)
                   .HasForeignKey(t => t.ScheduleId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // -------- ScheduleWeekDay --------
        modelBuilder.Entity<ScheduleWeekDay>(builder =>
        {
            builder.ToTable("ScheduleWeekDays");

            // Chave primária composta (ScheduleId, DayOfWeek)
            builder.HasKey(w => new { w.ScheduleId, w.DayOfWeek });

            builder.Property(w => w.DayOfWeek)
                   .IsRequired();

            builder.HasOne(w => w.Schedule)
                   .WithMany(s => s.WeekDays)
                   .HasForeignKey(w => w.ScheduleId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // -------- MedicineDoseOccurrence --------
        modelBuilder.Entity<MedicineDoseOccurrence>(builder =>
        {
            builder.ToTable("MedicineDoseOccurrences");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Status)
                   .HasConversion<int>() // DoseStatus enum
                   .IsRequired();

            builder.Property(d => d.ScheduledAt)
                   .IsRequired();

            builder.Property(d => d.TakenAt)
                   .HasConversion(
                        v => v.HasValue ? v.Value.ToUniversalTime() : (DateTimeOffset?)null,
                        v => v.HasValue ? v.Value.ToUniversalTime() : (DateTimeOffset?)null);

            builder.Property(d => d.SnoozedUntil)
                   .HasConversion(
                        v => v.HasValue ? v.Value.ToUniversalTime() : (DateTimeOffset?)null,
                        v => v.HasValue ? v.Value.ToUniversalTime() : (DateTimeOffset?)null);

            builder.Property(d => d.SkipReason)
                   .HasMaxLength(500);

            builder.Property(d => d.CreatedAt)
                   .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                   .IsRequired();

            builder.Property(d => d.UpdatedAt)
                   .HasConversion(
                        v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : (DateTime?)null,
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            builder.HasOne(d => d.Medicine)
                   .WithMany() // se tiver ICollection<MedicineDoseOccurrence> em Medicine, pode pôr .WithMany(m => m.DoseOccurrences)
                   .HasForeignKey(d => d.MedicineId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Schedule)
                   .WithMany() // se não tiver collection em Schedule
                   .HasForeignKey(d => d.ScheduleId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // -------- VitalSignRecord --------
        modelBuilder.Entity<VitalSignRecord>(builder =>
        {
            builder.ToTable("VitalSignRecords");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Type)
                   .HasConversion<int>() // VitalSignType enum
                   .IsRequired();

            builder.Property(v => v.Value)
                   .IsRequired();

            builder.Property(v => v.Unit)
                   .HasMaxLength(20);

            builder.Property(v => v.RecordedAt)
                   .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                   .IsRequired();

            builder.Property(v => v.Notes)
                   .HasMaxLength(1000);

            builder.Property(v => v.CreatedAt)
                   .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                   .IsRequired();

            builder.Property(v => v.UpdatedAt)
                   .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                   .IsRequired();

            builder.HasOne(v => v.Person)
                   .WithMany()
                   .HasForeignKey(v => v.PersonId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // -------- SymptomRecord --------
        modelBuilder.Entity<SymptomRecord>(builder =>
        {
            builder.ToTable("SymptomRecords");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Symptoms)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(s => s.Severity);

            builder.Property(s => s.RecordedAt)
                   .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                   .IsRequired();

            builder.Property(s => s.Notes)
                   .HasMaxLength(1000);

            builder.Property(s => s.CreatedAt)
                   .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                   .IsRequired();

            builder.Property(s => s.UpdatedAt)
                   .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                   .IsRequired();

            builder.HasOne(s => s.Person)
                   .WithMany()
                   .HasForeignKey(s => s.PersonId)
                   .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
