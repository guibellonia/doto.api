using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class ScheduleAdjustmentConfiguration : IEntityTypeConfiguration<ScheduleAdjustment>
{
    public void Configure(EntityTypeBuilder<ScheduleAdjustment> builder)
    {
        builder.ToTable("schedule_adjustments", t =>
        {
            t.HasCheckConstraint("ck_schedule_adjustments_occurrences_affected", "occurrences_affected >= 0");
            t.HasCheckConstraint(
                "ck_schedule_adjustments_version_progress",
                "new_generation_version > previous_generation_version");
        });

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Reason).IsRequired().HasConversion<string>().HasMaxLength(24);
        builder.Property(a => a.ShiftMinutes).IsRequired();
        builder.Property(a => a.EffectiveFromDate).IsRequired();
        builder.Property(a => a.OccurrencesAffected).IsRequired();
        builder.Property(a => a.PreviousGenerationVersion).IsRequired();
        builder.Property(a => a.NewGenerationVersion).IsRequired();
        builder.Property(a => a.AppliedAtUtc).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => new { a.ScheduleId, a.AppliedAtUtc })
            .IsDescending(false, true)
            .HasDatabaseName("ix_schedule_adjustments_schedule_id_applied_at_utc");

        builder.HasOne(a => a.Schedule)
            .WithMany()
            .HasForeignKey(a => a.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.TriggeredByOccurrence)
            .WithMany()
            .HasForeignKey(a => a.TriggeredByOccurrenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AppliedByUser)
            .WithMany()
            .HasForeignKey(a => a.AppliedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
