using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class VitalReadingConfiguration : IEntityTypeConfiguration<VitalReading>
{
    public void Configure(EntityTypeBuilder<VitalReading> builder)
    {
        builder.ToTable("vital_readings", t =>
        {
            t.HasCheckConstraint("ck_vital_readings_value_primary_positive", "value_primary > 0");

            t.HasCheckConstraint(
                "ck_vital_readings_blood_pressure_requires_diastolic",
                "type <> 'BloodPressure' OR value_secondary IS NOT NULL");

            t.HasCheckConstraint(
                "ck_vital_readings_secondary_only_blood_pressure",
                "type = 'BloodPressure' OR value_secondary IS NULL");
        });

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Type).IsRequired().HasConversion<string>().HasMaxLength(24);
        builder.Property(v => v.ValuePrimary).IsRequired().HasPrecision(8, 2);
        builder.Property(v => v.ValueSecondary).HasPrecision(8, 2);
        builder.Property(v => v.Unit).IsRequired().HasMaxLength(16);
        builder.Property(v => v.Context).IsRequired().HasConversion<string>().HasMaxLength(24);
        builder.Property(v => v.MeasuredAtUtc).IsRequired();
        builder.Property(v => v.MeasuredLocalDate).IsRequired();
        builder.Property(v => v.MeasuredLocalTime).IsRequired();
        builder.Property(v => v.CreatedAt).IsRequired();

        builder.HasIndex(v => new { v.UserId, v.Type, v.MeasuredAtUtc })
            .IsDescending(false, false, true)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_vital_readings_user_id_type_measured_at_utc");

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
