using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class MedicationSchedule : AuditableEntity, ISoftDeletable
{
    private readonly List<ScheduleTimeSlot> _timeSlots = [];

    private MedicationSchedule()
    {
    }

    private MedicationSchedule(
        Guid medicationId,
        Guid userId,
        RecurrenceType recurrenceType,
        DayOfWeekFlags daysOfWeek,
        short? dayOfMonth,
        DateOnly startDate,
        DateOnly? endDate,
        short dosesPerDay,
        short lateGraceMinutes,
        TimeOnly? anchorTime,
        short? intervalMinutes,
        short? minIntervalMinutes)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (medicationId == Guid.Empty)
        {
            throw new ArgumentException("O agendamento precisa de um medicamento.", nameof(medicationId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("O agendamento precisa de um usuário.", nameof(userId));
        }

        MedicationId = medicationId;
        UserId = userId;
        RecurrenceType = recurrenceType;
        DaysOfWeek = daysOfWeek;
        DayOfMonth = dayOfMonth;
        StartDate = startDate;
        EndDate = recurrenceType == RecurrenceType.SingleDose ? startDate : endDate;
        DosesPerDay = dosesPerDay;
        LateGraceMinutes = lateGraceMinutes;
        AnchorTime = anchorTime;
        IntervalMinutes = intervalMinutes;
        MinIntervalMinutes = minIntervalMinutes ?? intervalMinutes;
        GenerationVersion = 1;
        IsActive = true;

        Validate();
    }

    public Guid MedicationId { get; private set; }

    public Guid UserId { get; private set; }

    public RecurrenceType RecurrenceType { get; private set; }

    public DayOfWeekFlags DaysOfWeek { get; private set; }

    public short? DayOfMonth { get; private set; }

    public TimeOnly? AnchorTime { get; private set; }

    public short? IntervalMinutes { get; private set; }

    public short? MinIntervalMinutes { get; private set; }

    public short DosesPerDay { get; private set; }

    public short LateGraceMinutes { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public int GenerationVersion { get; private set; }

    public DateOnly? GeneratedThroughDate { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public Medication? Medication { get; private set; }

    public AppUser? User { get; private set; }

    public IReadOnlyCollection<ScheduleTimeSlot> TimeSlots => _timeSlots;

    public static MedicationSchedule Create(
        Guid medicationId,
        Guid userId,
        RecurrenceType recurrenceType,
        DateOnly startDate,
        short dosesPerDay,
        short lateGraceMinutes,
        DayOfWeekFlags daysOfWeek = DayOfWeekFlags.None,
        short? dayOfMonth = null,
        DateOnly? endDate = null)
        => new(
            medicationId,
            userId,
            recurrenceType,
            daysOfWeek,
            dayOfMonth,
            startDate,
            endDate,
            dosesPerDay,
            lateGraceMinutes,
            anchorTime: null,
            intervalMinutes: null,
            minIntervalMinutes: null);

    public static MedicationSchedule CreateWithInterval(
        Guid medicationId,
        Guid userId,
        RecurrenceType recurrenceType,
        DateOnly startDate,
        TimeOnly anchorTime,
        short intervalMinutes,
        short dosesPerDay,
        short lateGraceMinutes,
        short? minIntervalMinutes = null,
        DayOfWeekFlags daysOfWeek = DayOfWeekFlags.None,
        short? dayOfMonth = null,
        DateOnly? endDate = null)
        => new(
            medicationId,
            userId,
            recurrenceType,
            daysOfWeek,
            dayOfMonth,
            startDate,
            endDate,
            dosesPerDay,
            lateGraceMinutes,
            anchorTime,
            intervalMinutes,
            minIntervalMinutes);

    public void ReplaceTimeSlots(IEnumerable<TimeOnly> timesOfDay)
    {
        var ordered = timesOfDay.Distinct().OrderBy(t => t).ToList();
        if (ordered.Count == 0)
        {
            throw new ArgumentException("O agendamento precisa de ao menos um horário.", nameof(timesOfDay));
        }

        _timeSlots.Clear();
        for (var i = 0; i < ordered.Count; i++)
        {
            _timeSlots.Add(ScheduleTimeSlot.Create(Id, ordered[i], (short)i));
        }

        DosesPerDay = (short)ordered.Count;
        Touch();
    }

    public void ChangeInterval(TimeOnly anchorTime, short intervalMinutes, short dosesPerDay, short? minIntervalMinutes = null)
    {
        AnchorTime = anchorTime;
        IntervalMinutes = intervalMinutes;
        DosesPerDay = dosesPerDay;
        MinIntervalMinutes = minIntervalMinutes ?? intervalMinutes;
        Validate();
        Touch();
    }

    public int BumpGenerationVersion()
    {
        GenerationVersion++;
        Touch();
        return GenerationVersion;
    }

    public void AdvanceGenerationHorizon(DateOnly generatedThroughDate)
    {
        if (GeneratedThroughDate is null || generatedThroughDate > GeneratedThroughDate)
        {
            GeneratedThroughDate = generatedThroughDate;
            Touch();
        }
    }

    public void UpdatePeriod(DateOnly startDate, DateOnly? endDate)
    {
        if (endDate is not null && endDate < startDate)
        {
            throw new ArgumentException("O fim não pode ser anterior ao início.", nameof(endDate));
        }

        StartDate = startDate;
        EndDate = RecurrenceType == RecurrenceType.SingleDose ? startDate : endDate;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch();
    }

    public void SoftDelete(DateTime deletedAtUtc)
    {
        if (DeletedAt is not null)
        {
            return;
        }

        DeletedAt = deletedAtUtc;
        IsActive = false;
        Touch();
    }

    private void Validate()
    {
        if (DosesPerDay < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(DosesPerDay), "O agendamento precisa de ao menos uma dose por dia.");
        }

        if (LateGraceMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(LateGraceMinutes), "A tolerância não pode ser negativa.");
        }

        if (EndDate is not null && EndDate < StartDate)
        {
            throw new ArgumentException("O fim do agendamento não pode ser anterior ao início.");
        }

        if ((int)DaysOfWeek is < 0 or > (int)DayOfWeekFlags.EveryDay)
        {
            throw new ArgumentOutOfRangeException(nameof(DaysOfWeek), "A máscara de dias da semana deve estar entre 0 e 127.");
        }

        switch (RecurrenceType)
        {
            case RecurrenceType.SpecificWeekDays when DaysOfWeek == DayOfWeekFlags.None:
                throw new ArgumentException("Dias específicos exigem ao menos um dia da semana selecionado.");

            case RecurrenceType.Weekly when int.PopCount((int)DaysOfWeek) != 1:
                throw new ArgumentException("O tipo semanal exige exatamente um dia da semana.");

            case RecurrenceType.Monthly when DayOfMonth is null or < 1 or > 31:
                throw new ArgumentException("O tipo mensal exige um dia do mês entre 1 e 31.");
        }

        if (IntervalMinutes is not null)
        {
            if (AnchorTime is null)
            {
                throw new ArgumentException("Um intervalo declarado exige um horário âncora.");
            }

            if (IntervalMinutes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(IntervalMinutes), "O intervalo deve ser positivo.");
            }

            if (MinIntervalMinutes is <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(MinIntervalMinutes), "O intervalo mínimo deve ser positivo.");
            }
        }
    }
}
