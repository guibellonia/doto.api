using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class HealthConditionConfiguration : IEntityTypeConfiguration<HealthCondition>
{
    public void Configure(EntityTypeBuilder<HealthCondition> builder)
    {
        builder.ToTable("health_conditions");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(160);
        builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => c.UserId)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_health_conditions_user_id");

        builder.HasOne(c => c.User)
            .WithMany(u => u.HealthConditions)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
