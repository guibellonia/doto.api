using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class ReportExportConfiguration : IEntityTypeConfiguration<ReportExport>
{
    public void Configure(EntityTypeBuilder<ReportExport> builder)
    {
        builder.ToTable("report_exports", t =>
            t.HasCheckConstraint("ck_report_exports_period", "period_end >= period_start"));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReportType).IsRequired().HasConversion<string>().HasMaxLength(24);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(r => r.PeriodStart).IsRequired();
        builder.Property(r => r.PeriodEnd).IsRequired();
        builder.Property(r => r.FileName).HasMaxLength(255);
        builder.Property(r => r.StoragePath).HasMaxLength(512);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => new { r.SubjectUserId, r.GeneratedAtUtc })
            .IsDescending(false, true)
            .HasDatabaseName("ix_report_exports_subject_user_id_generated_at_utc");

        builder.HasOne(r => r.RequestedByUser)
            .WithMany()
            .HasForeignKey(r => r.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.SubjectUser)
            .WithMany()
            .HasForeignKey(r => r.SubjectUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
