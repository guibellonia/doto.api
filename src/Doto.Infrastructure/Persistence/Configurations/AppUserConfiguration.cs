using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("app_users", t =>
        {
            t.HasCheckConstraint(
                "ck_app_users_role_parent_coherence",
                "(role = 'Child' AND parent_id IS NOT NULL) OR (role = 'Parent' AND parent_id IS NULL)");

            t.HasCheckConstraint("ck_app_users_no_self_parent", "parent_id IS DISTINCT FROM id");
        });

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Email).IsRequired().HasMaxLength(320);
        builder.Property(u => u.Username).IsRequired().HasMaxLength(64);
        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(160);
        builder.Property(u => u.AvatarUrl).HasMaxLength(512);

        builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(u => u.Status).IsRequired().HasConversion<string>().HasMaxLength(24);

        builder.Property(u => u.MustChangePassword).IsRequired().HasDefaultValue(false);

        builder.Property(u => u.WeightKg).HasPrecision(5, 2);
        builder.Property(u => u.HeightCm).HasPrecision(5, 1);

        builder.Property(u => u.TimeZone)
            .IsRequired()
            .HasMaxLength(64)
            .HasDefaultValue(AppUser.DefaultTimeZone);

        builder.Property(u => u.CreatedAt).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("ux_app_users_email");
        builder.HasIndex(u => u.Username).IsUnique().HasDatabaseName("ux_app_users_username");

        builder.HasIndex(u => u.ParentId)
            .HasFilter("parent_id IS NOT NULL")
            .HasDatabaseName("ix_app_users_parent_id");

        builder.HasIndex(u => u.Role).HasDatabaseName("ix_app_users_role");

        builder.HasOne(u => u.Parent)
            .WithMany(u => u.Children)
            .HasForeignKey(u => u.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(u => u.Children).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.HealthConditions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
