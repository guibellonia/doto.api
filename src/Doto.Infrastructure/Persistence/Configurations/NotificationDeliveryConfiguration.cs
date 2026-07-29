using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("notification_deliveries", t =>
            t.HasCheckConstraint("ck_notification_deliveries_attempt_count", "attempt_count >= 0"));

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Kind).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(d => d.Status).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(d => d.ScheduledForUtc).IsRequired();
        builder.Property(d => d.AttemptCount).IsRequired().HasDefaultValue(0);
        builder.Property(d => d.ProviderMessageId).HasMaxLength(128);
        builder.Property(d => d.CreatedAt).IsRequired();

        builder.Property(d => d.Payload).HasColumnType("jsonb");

        builder.HasIndex(d => new { d.Status, d.ScheduledForUtc })
            .HasFilter("status = 'Scheduled'")
            .HasDatabaseName("ix_notification_deliveries_scheduled");

        builder.HasIndex(d => new { d.RecipientUserId, d.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("ix_notification_deliveries_recipient_user_id_created_at");

        builder.HasIndex(d => d.DoseOccurrenceId)
            .HasDatabaseName("ix_notification_deliveries_dose_occurrence_id");

        builder.HasOne(d => d.RecipientUser)
            .WithMany()
            .HasForeignKey(d => d.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Device)
            .WithMany()
            .HasForeignKey(d => d.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.NotificationPreference)
            .WithMany()
            .HasForeignKey(d => d.NotificationPreferenceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.DoseOccurrence)
            .WithMany()
            .HasForeignKey(d => d.DoseOccurrenceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
