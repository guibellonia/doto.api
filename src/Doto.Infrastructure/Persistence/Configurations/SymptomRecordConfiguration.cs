using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class SymptomRecordConfiguration : IEntityTypeConfiguration<SymptomRecord>
{
    public void Configure(EntityTypeBuilder<SymptomRecord> builder)
    {
        builder.ToTable("symptom_records", t =>
        {
            t.HasCheckConstraint("ck_symptom_records_severity_range", "severity IS NULL OR severity BETWEEN 0 AND 10");
            t.HasCheckConstraint("ck_symptom_records_duration", "duration_minutes IS NULL OR duration_minutes >= 0");
        });

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(120);
        builder.Property(s => s.Trend).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(s => s.RecordedAtUtc).IsRequired();
        builder.Property(s => s.RecordedLocalDate).IsRequired();
        builder.Property(s => s.RecordedLocalTime).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasIndex(s => new { s.UserId, s.RecordedAtUtc })
            .IsDescending(false, true)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_symptom_records_user_id_recorded_at_utc");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
