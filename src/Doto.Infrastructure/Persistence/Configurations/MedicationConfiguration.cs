using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class MedicationConfiguration : IEntityTypeConfiguration<Medication>
{
    public void Configure(EntityTypeBuilder<Medication> builder)
    {
        builder.ToTable("medications", t =>
        {
            t.HasCheckConstraint("ck_medications_dosage_positive", "dosage_value > 0");
            t.HasCheckConstraint(
                "ck_medications_treatment_period",
                "treatment_end_date IS NULL OR treatment_end_date >= treatment_start_date");
        });

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
        builder.Property(m => m.DosageValue).IsRequired().HasPrecision(10, 3);
        builder.Property(m => m.DosageUnit).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(m => m.TreatmentStartDate).IsRequired();
        builder.Property(m => m.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(m => m.CreatedAt).IsRequired();

        builder.HasIndex(m => new { m.UserId, m.IsActive })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_medications_user_id_is_active");

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(m => m.Schedules).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
