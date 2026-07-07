using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class MedicineDoseOccurrence
{
    #region Properties

    public Guid Id { get; private set; }

    public Guid MedicineId { get; private set; }
    public Medicine Medicine { get; private set; } = null!;

    public Guid ScheduleId { get; private set; }
    public Schedule Schedule { get; private set; } = null!;

    public DateTimeOffset ScheduledAt { get; private set; }

    public DoseStatus Status { get; private set; } = DoseStatus.Pending;

    public DateTimeOffset? TakenAt { get; private set; }

    public DateTimeOffset? SnoozedUntil { get; private set; }

    public string? SkipReason { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    #endregion

    protected MedicineDoseOccurrence() { }

    public MedicineDoseOccurrence(
        Guid id,
        Guid medicineId,
        Guid scheduleId,
        DateTimeOffset scheduledAt)
    {
        Id = id;
        MedicineId = medicineId;
        ScheduleId = scheduleId;
        ScheduledAt = scheduledAt;
        Status = DoseStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    #region Methods
    public void MarkTaken(DateTimeOffset takenAt)
    {
        Status = DoseStatus.Taken;
        TakenAt = takenAt;
        SnoozedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSkipped(string? reason = null)
    {
        if (Status == DoseStatus.Taken)
            throw new InvalidOperationException("Cannot skip a dose that was already taken.");

        Status = DoseStatus.Skipped;
        SkipReason = reason;
        SnoozedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        if (Status is DoseStatus.Taken or DoseStatus.Skipped)
            return;

        Status = DoseStatus.Expired;
        SnoozedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SnoozeUntil(DateTimeOffset snoozedUntil)
    {
        if (Status != DoseStatus.Pending)
            throw new InvalidOperationException("Only pending doses can be snoozed.");

        SnoozedUntil = snoozedUntil;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion
}
