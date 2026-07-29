using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class DoseOccurrence : EntityBase
{
    private DoseOccurrence()
    {
    }

    private DoseOccurrence(
        Guid userId,
        Guid medicationId,
        Guid scheduleId,
        DateOnly scheduledLocalDate,
        TimeOnly scheduledLocalTime,
        DateTime scheduledAtUtc,
        int occurrenceIndex,
        int generationVersion)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A ocorrência precisa de um usuário.", nameof(userId));
        }

        if (occurrenceIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(occurrenceIndex), "O índice da ocorrência não pode ser negativo.");
        }

        if (generationVersion < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(generationVersion), "A versão de geração começa em 1.");
        }

        UserId = userId;
        MedicationId = medicationId;
        ScheduleId = scheduleId;
        ScheduledLocalDate = scheduledLocalDate;
        ScheduledLocalTime = scheduledLocalTime;
        ScheduledAtUtc = scheduledAtUtc;
        OriginalScheduledAtUtc = scheduledAtUtc;
        OccurrenceIndex = occurrenceIndex;
        GenerationVersion = generationVersion;
        Status = DoseStatus.Pending;
        RecordSource = DoseRecordSource.System;
        IsRetroactive = false;
    }

    public Guid UserId { get; private set; }

    public Guid MedicationId { get; private set; }

    public Guid ScheduleId { get; private set; }

    public DateOnly ScheduledLocalDate { get; private set; }

    public TimeOnly ScheduledLocalTime { get; private set; }

    public DateTime ScheduledAtUtc { get; private set; }

    public DateTime OriginalScheduledAtUtc { get; private set; }

    public DoseStatus Status { get; private set; }

    public DateTime? TakenAtUtc { get; private set; }

    public int? DelayMinutes { get; private set; }

    public bool IsRetroactive { get; private set; }

    public Guid? RecordedByUserId { get; private set; }

    public DoseRecordSource RecordSource { get; private set; }

    public int OccurrenceIndex { get; private set; }

    public int GenerationVersion { get; private set; }

    public Guid? SupersededByOccurrenceId { get; private set; }

    public string? Notes { get; private set; }

    public AppUser? User { get; private set; }

    public Medication? Medication { get; private set; }

    public MedicationSchedule? Schedule { get; private set; }

    public AppUser? RecordedByUser { get; private set; }

    public DoseOccurrence? SupersededByOccurrence { get; private set; }

    public static DoseOccurrence Materialize(
        Guid userId,
        Guid medicationId,
        Guid scheduleId,
        DateOnly scheduledLocalDate,
        TimeOnly scheduledLocalTime,
        DateTime scheduledAtUtc,
        int occurrenceIndex,
        int generationVersion)
        => new(
            userId,
            medicationId,
            scheduleId,
            scheduledLocalDate,
            scheduledLocalTime,
            scheduledAtUtc,
            occurrenceIndex,
            generationVersion);

    public void MarkTaken(DateTime atUtc, Guid byUserId, DoseRecordSource source, bool isRetroactive = false, string? notes = null)
    {
        EnsureOpen();

        Status = DoseStatus.Taken;
        TakenAtUtc = atUtc;
        DelayMinutes = (int)Math.Round((atUtc - ScheduledAtUtc).TotalMinutes);
        RecordedByUserId = byUserId;
        RecordSource = source;
        IsRetroactive = isRetroactive;
        Notes = string.IsNullOrWhiteSpace(notes) ? Notes : notes.Trim();
    }

    public void MarkSkipped(Guid byUserId, DoseRecordSource source, string? notes = null)
    {
        EnsureOpen();

        Status = DoseStatus.Skipped;
        RecordedByUserId = byUserId;
        RecordSource = source;
        Notes = string.IsNullOrWhiteSpace(notes) ? Notes : notes.Trim();
    }

    public void MarkMissed()
    {
        EnsureOpen();

        Status = DoseStatus.Missed;
        RecordSource = DoseRecordSource.System;
    }

    public void Cancel()
    {
        EnsureOpen();
        Status = DoseStatus.Cancelled;
    }

    public void SupersedeWith(Guid replacementOccurrenceId)
    {
        if (Status != DoseStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Só uma dose pendente pode ser substituída (status atual: {Status}). Ocorrências resolvidas são imutáveis.");
        }

        Status = DoseStatus.Superseded;
        SupersededByOccurrenceId = replacementOccurrenceId;
    }

    public void RecomputeScheduledInstant(DateTime scheduledAtUtc)
    {
        if (Status != DoseStatus.Pending)
        {
            return;
        }

        ScheduledAtUtc = scheduledAtUtc;
    }

    public void Reschedule(DateOnly localDate, TimeOnly localTime, DateTime scheduledAtUtc)
    {
        if (Status != DoseStatus.Pending)
        {
            throw new InvalidOperationException("Só uma dose pendente pode ser reagendada.");
        }

        ScheduledLocalDate = localDate;
        ScheduledLocalTime = localTime;
        ScheduledAtUtc = scheduledAtUtc;
    }

    public bool CountsTowardAdherence =>
        Status is DoseStatus.Taken or DoseStatus.Missed or DoseStatus.Skipped;

    private void EnsureOpen()
    {
        if (Status != DoseStatus.Pending)
        {
            throw new InvalidOperationException($"A dose já foi resolvida (status atual: {Status}).");
        }
    }
}
