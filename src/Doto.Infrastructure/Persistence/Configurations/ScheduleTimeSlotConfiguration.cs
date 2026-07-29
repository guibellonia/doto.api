using Doto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doto.Infrastructure.Persistence.Configurations;

public class ScheduleTimeSlotConfiguration : IEntityTypeConfiguration<ScheduleTimeSlot>
{
    public void Configure(EntityTypeBuilder<ScheduleTimeSlot> builder)
    {
        builder.ToTable("schedule_time_slots", t =>
            t.HasCheckConstraint("ck_schedule_time_slots_slot_order", "slot_order >= 0"));

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TimeOfDay).IsRequired();
        builder.Property(s => s.SlotOrder).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasIndex(s => new { s.ScheduleId, s.SlotOrder })
            .IsUnique()
            .HasDatabaseName("ux_schedule_time_slots_schedule_id_slot_order");

        builder.HasOne(s => s.Schedule)
            .WithMany(x => x.TimeSlots)
            .HasForeignKey(s => s.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
