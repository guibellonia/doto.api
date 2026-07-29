using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class MedicationScheduleConfiguration : IEntityTypeConfiguration<MedicationSchedule>
{
    public void Configure(EntityTypeBuilder<MedicationSchedule> builder)
    {
        builder.ToTable("medication_schedules", t =>
        {
            t.HasCheckConstraint("ck_medication_schedules_days_of_week_range", "days_of_week BETWEEN 0 AND 127");

            t.HasCheckConstraint(
                "ck_medication_schedules_weekdays_required",
                "recurrence_type <> 'SpecificWeekDays' OR days_of_week > 0");

            t.HasCheckConstraint(
                "ck_medication_schedules_weekly_single_day",
                "recurrence_type <> 'Weekly' OR (days_of_week > 0 AND (days_of_week & (days_of_week - 1)) = 0)");

            t.HasCheckConstraint(
                "ck_medication_schedules_day_of_month",
                "(recurrence_type = 'Monthly' AND day_of_month BETWEEN 1 AND 31) OR (recurrence_type <> 'Monthly' AND day_of_month IS NULL)");

            t.HasCheckConstraint(
                "ck_medication_schedules_interval_coherence",
                "interval_minutes IS NULL OR (anchor_time IS NOT NULL AND doses_per_day >= 1 AND interval_minutes > 0)");

            t.HasCheckConstraint("ck_medication_schedules_doses_per_day", "doses_per_day >= 1");
            t.HasCheckConstraint("ck_medication_schedules_late_grace", "late_grace_minutes >= 0");
            t.HasCheckConstraint("ck_medication_schedules_period", "end_date IS NULL OR end_date >= start_date");
        });

        builder.HasKey(s => s.Id);

        builder.Property(s => s.RecurrenceType).IsRequired().HasConversion<string>().HasMaxLength(24);

        builder.Property(s => s.DaysOfWeek).IsRequired().HasConversion<int>();

        builder.Property(s => s.DosesPerDay).IsRequired();
        builder.Property(s => s.LateGraceMinutes).IsRequired();
        builder.Property(s => s.StartDate).IsRequired();
        builder.Property(s => s.GenerationVersion).IsRequired().HasDefaultValue(1);
        builder.Property(s => s.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasIndex(s => s.MedicationId).HasDatabaseName("ix_medication_schedules_medication_id");

        builder.HasIndex(s => s.GeneratedThroughDate)
            .HasFilter("is_active")
            .HasDatabaseName("ix_medication_schedules_generated_through_date");

        builder.HasOne(s => s.Medication)
            .WithMany(m => m.Schedules)
            .HasForeignKey(s => s.MedicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(s => s.TimeSlots).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
