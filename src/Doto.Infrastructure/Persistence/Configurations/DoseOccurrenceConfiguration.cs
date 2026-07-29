using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class DoseOccurrenceConfiguration : IEntityTypeConfiguration<DoseOccurrence>
{
    public void Configure(EntityTypeBuilder<DoseOccurrence> builder)
    {
        builder.ToTable("dose_occurrences", t =>
        {
            t.HasCheckConstraint("ck_dose_occurrences_occurrence_index", "occurrence_index >= 0");
            t.HasCheckConstraint("ck_dose_occurrences_generation_version", "generation_version >= 1");
            t.HasCheckConstraint(
                "ck_dose_occurrences_taken_requires_instant",
                "status <> 'Taken' OR taken_at_utc IS NOT NULL");
            t.HasCheckConstraint(
                "ck_dose_occurrences_superseded_requires_target",
                "status <> 'Superseded' OR superseded_by_occurrence_id IS NOT NULL");
        });

        builder.HasKey(o => o.Id);

        builder.Property(o => o.ScheduledLocalDate).IsRequired();
        builder.Property(o => o.ScheduledLocalTime).IsRequired();

        builder.Property(o => o.ScheduledAtUtc).IsRequired();
        builder.Property(o => o.OriginalScheduledAtUtc).IsRequired();

        builder.Property(o => o.Status).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(o => o.RecordSource).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(o => o.IsRetroactive).IsRequired().HasDefaultValue(false);
        builder.Property(o => o.OccurrenceIndex).IsRequired();
        builder.Property(o => o.GenerationVersion).IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();

        builder.HasIndex(o => new { o.UserId, o.ScheduledLocalDate })
            .HasDatabaseName("ix_dose_occurrences_user_id_scheduled_local_date");

        builder.HasIndex(o => new { o.MedicationId, o.ScheduledAtUtc })
            .HasDatabaseName("ix_dose_occurrences_medication_id_scheduled_at_utc");

        builder.HasIndex(o => new { o.Status, o.ScheduledAtUtc })
            .HasFilter("status = 'Pending'")
            .HasDatabaseName("ix_dose_occurrences_pending_scheduled_at_utc");

        builder.HasIndex(o => new { o.ScheduleId, o.GenerationVersion, o.OccurrenceIndex })
            .IsUnique()
            .HasDatabaseName("ux_dose_occurrences_schedule_generation_index");

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Medication)
            .WithMany()
            .HasForeignKey(o => o.MedicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Schedule)
            .WithMany()
            .HasForeignKey(o => o.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.RecordedByUser)
            .WithMany()
            .HasForeignKey(o => o.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.SupersededByOccurrence)
            .WithMany()
            .HasForeignKey(o => o.SupersededByOccurrenceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
