using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class ScheduleAdjustment : EntityBase
{
    private ScheduleAdjustment()
    {
    }

    private ScheduleAdjustment(
        Guid scheduleId,
        AdjustmentReason reason,
        int shiftMinutes,
        DateOnly effectiveFromDate,
        int occurrencesAffected,
        int previousGenerationVersion,
        int newGenerationVersion,
        Guid? triggeredByOccurrenceId,
        Guid? appliedByUserId,
        DateTime appliedAtUtc,
        string? notes)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (scheduleId == Guid.Empty)
        {
            throw new ArgumentException("O ajuste precisa de um agendamento.", nameof(scheduleId));
        }

        if (occurrencesAffected < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(occurrencesAffected), "A contagem de ocorrências não pode ser negativa.");
        }

        if (newGenerationVersion <= previousGenerationVersion)
        {
            throw new ArgumentException(
                "A nova versão de geração deve ser maior que a anterior.",
                nameof(newGenerationVersion));
        }

        ScheduleId = scheduleId;
        Reason = reason;
        ShiftMinutes = shiftMinutes;
        EffectiveFromDate = effectiveFromDate;
        OccurrencesAffected = occurrencesAffected;
        PreviousGenerationVersion = previousGenerationVersion;
        NewGenerationVersion = newGenerationVersion;
        TriggeredByOccurrenceId = triggeredByOccurrenceId;
        AppliedByUserId = appliedByUserId;
        AppliedAtUtc = appliedAtUtc;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public Guid ScheduleId { get; private set; }

    public Guid? TriggeredByOccurrenceId { get; private set; }

    public AdjustmentReason Reason { get; private set; }

    public int ShiftMinutes { get; private set; }

    public DateOnly EffectiveFromDate { get; private set; }

    public int OccurrencesAffected { get; private set; }

    public int PreviousGenerationVersion { get; private set; }

    public int NewGenerationVersion { get; private set; }

    public Guid? AppliedByUserId { get; private set; }

    public DateTime AppliedAtUtc { get; private set; }

    public string? Notes { get; private set; }

    public MedicationSchedule? Schedule { get; private set; }

    public DoseOccurrence? TriggeredByOccurrence { get; private set; }

    public AppUser? AppliedByUser { get; private set; }

    public static ScheduleAdjustment Record(
        Guid scheduleId,
        AdjustmentReason reason,
        int shiftMinutes,
        DateOnly effectiveFromDate,
        int occurrencesAffected,
        int previousGenerationVersion,
        int newGenerationVersion,
        DateTime appliedAtUtc,
        Guid? triggeredByOccurrenceId = null,
        Guid? appliedByUserId = null,
        string? notes = null)
        => new(
            scheduleId,
            reason,
            shiftMinutes,
            effectiveFromDate,
            occurrencesAffected,
            previousGenerationVersion,
            newGenerationVersion,
            triggeredByOccurrenceId,
            appliedByUserId,
            appliedAtUtc,
            notes);
}
