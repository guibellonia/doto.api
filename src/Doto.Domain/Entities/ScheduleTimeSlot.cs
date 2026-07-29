using Doto.Domain.Common;

namespace Doto.Domain.Entities;

public class ScheduleTimeSlot : EntityBase
{
    private ScheduleTimeSlot()
    {
    }

    private ScheduleTimeSlot(Guid scheduleId, TimeOnly timeOfDay, short slotOrder)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (slotOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slotOrder), "A ordem do horário não pode ser negativa.");
        }

        ScheduleId = scheduleId;
        TimeOfDay = timeOfDay;
        SlotOrder = slotOrder;
    }

    public Guid ScheduleId { get; private set; }

    public TimeOnly TimeOfDay { get; private set; }

    public short SlotOrder { get; private set; }

    public MedicationSchedule? Schedule { get; private set; }

    internal static ScheduleTimeSlot Create(Guid scheduleId, TimeOnly timeOfDay, short slotOrder)
        => new(scheduleId, timeOfDay, slotOrder);
}
