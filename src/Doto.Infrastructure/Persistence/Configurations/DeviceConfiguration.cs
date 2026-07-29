using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.PushToken).IsRequired().HasMaxLength(255);
        builder.Property(d => d.Platform).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(d => d.DeviceName).HasMaxLength(120);
        builder.Property(d => d.AppVersion).HasMaxLength(32);
        builder.Property(d => d.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(d => d.CreatedAt).IsRequired();

        builder.HasIndex(d => d.PushToken).IsUnique().HasDatabaseName("ux_devices_push_token");

        builder.HasIndex(d => d.UserId)
            .HasFilter("is_active")
            .HasDatabaseName("ix_devices_user_id");

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
