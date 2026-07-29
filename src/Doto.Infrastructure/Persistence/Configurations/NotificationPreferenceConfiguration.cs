using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences", t =>
        {
            t.HasCheckConstraint("ck_notification_preferences_lead_minutes", "lead_minutes >= 0");
            t.HasCheckConstraint(
                "ck_notification_preferences_repeat",
                "repeat_interval_minutes IS NULL OR repeat_interval_minutes > 0");
            t.HasCheckConstraint(
                "ck_notification_preferences_quiet_hours",
                "(quiet_hours_start IS NULL) = (quiet_hours_end IS NULL)");
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Kind).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.Channel).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(p => p.Enabled).IsRequired().HasDefaultValue(true);
        builder.Property(p => p.LeadMinutes).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => new { p.SubjectUserId, p.RecipientUserId, p.MedicationId, p.Kind })
            .IsUnique()
            .HasFilter("medication_id IS NOT NULL AND deleted_at IS NULL")
            .HasDatabaseName("ux_notification_preferences_medication_override");

        builder.HasIndex(p => new { p.SubjectUserId, p.RecipientUserId, p.Kind })
            .IsUnique()
            .HasFilter("medication_id IS NULL AND deleted_at IS NULL")
            .HasDatabaseName("ux_notification_preferences_default");

        builder.HasOne(p => p.SubjectUser)
            .WithMany()
            .HasForeignKey(p => p.SubjectUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.RecipientUser)
            .WithMany()
            .HasForeignKey(p => p.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Medication)
            .WithMany()
            .HasForeignKey(p => p.MedicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
