using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class ChildInviteConfiguration : IEntityTypeConfiguration<ChildInvite>
{
    public void Configure(EntityTypeBuilder<ChildInvite> builder)
    {
        builder.ToTable("child_invites");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvitedEmail).IsRequired().HasMaxLength(320);
        builder.Property(i => i.Status).IsRequired().HasConversion<string>().HasMaxLength(16);

        builder.Property(i => i.InviteTokenHash).HasMaxLength(64);

        builder.Property(i => i.ExpiresAtUtc).IsRequired();
        builder.Property(i => i.ResentCount).IsRequired().HasDefaultValue(0);
        builder.Property(i => i.CreatedAt).IsRequired();

        builder.HasIndex(i => new { i.ParentUserId, i.InvitedEmail })
            .IsUnique()
            .HasFilter("status = 'Pending'")
            .HasDatabaseName("ux_child_invites_pending_email");

        builder.HasIndex(i => i.ChildUserId).HasDatabaseName("ix_child_invites_child_user_id");

        builder.HasIndex(i => new { i.Status, i.ExpiresAtUtc })
            .HasDatabaseName("ix_child_invites_status_expires_at_utc");

        builder.HasOne(i => i.ParentUser)
            .WithMany()
            .HasForeignKey(i => i.ParentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ChildUser)
            .WithMany()
            .HasForeignKey(i => i.ChildUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
