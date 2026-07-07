using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class Schedule
{
    public Guid Id { get; private set; }

    public Guid MedicineId { get; private set; }
    public Medicine? Medicine { get; private set; }

    public MedicineScheduleType ScheduleType { get; private set; }

    // --- OncePerDay ---
    public TimeOnly? TimeOfDay { get; private set; }

    // --- MultipleFixedTimesPerDay ---
    public ICollection<ScheduleTime> TimesOfDay { get; private set; } = new List<ScheduleTime>();

    // --- EveryXHours ---
    public int? IntervalInHours { get; private set; }
    public DateTimeOffset? FirstDoseAt { get; private set; }

    // --- SpecificWeekDays ---
    public ICollection<ScheduleWeekDay> WeekDays { get; private set; } = new List<ScheduleWeekDay>();

    public int? PreAlarmMinutes { get; private set; }
    public int? PosAlarmMinutes { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsActive { get; private set; } = true;

    protected Schedule() { }

    public Schedule(Guid id, Guid medicineId, MedicineScheduleType scheduleType)
    {
        Id = id;
        MedicineId = medicineId;
        ScheduleType = scheduleType;
        IsActive = true;
    }

    #region Factory Methods

    public void SetOncePerDay(TimeOnly time)
    {
        ScheduleType = MedicineScheduleType.OncePerDay;
        TimeOfDay = time;
        TimesOfDay.Clear();
        IntervalInHours = null;
        FirstDoseAt = null;
        WeekDays.Clear();
    }

    public void SetMultipleTimesPerDay(IEnumerable<TimeOnly> times)
    {
        ScheduleType = MedicineScheduleType.MultipleFixedTimesPerDay;
        TimeOfDay = null;
        TimesOfDay = times.Select(t => new ScheduleTime(Guid.NewGuid(), Id, t)).ToList();
        IntervalInHours = null;
        FirstDoseAt = null;
        WeekDays.Clear();
    }

    public void SetEveryXHours(int intervalInHours, DateTimeOffset firstDoseAt)
    {
        if (intervalInHours <= 0)
            throw new ArgumentException("Interval must be >= 1", nameof(intervalInHours));

        ScheduleType = MedicineScheduleType.EveryXHours;
        IntervalInHours = intervalInHours;
        FirstDoseAt = firstDoseAt;

        TimeOfDay = null;
        TimesOfDay.Clear();
        WeekDays.Clear();
    }

    public void SetSpecificWeekDays(TimeOnly time, IEnumerable<int> days)
    {
        ScheduleType = MedicineScheduleType.SpecificWeekDays;

        TimeOfDay = time;
        WeekDays = days.Select(d => new ScheduleWeekDay(Id, d)).ToList();

        TimesOfDay.Clear();
        IntervalInHours = null;
        FirstDoseAt = null;
    }

    public void SetAsNeeded()
    {
        ScheduleType = MedicineScheduleType.AsNeeded;

        TimeOfDay = null;
        TimesOfDay.Clear();
        WeekDays.Clear();
        IntervalInHours = null;
        FirstDoseAt = null;
    }

    #endregion

    #region Config Methods
    public void SetAlarmConfig(int? preAlarmMinutes, int? posAlarmMinutes)
    {
        PreAlarmMinutes = preAlarmMinutes;
        PosAlarmMinutes = posAlarmMinutes;
    }

    public void ChangeType(MedicineScheduleType type)
    {
        if (ScheduleType == type)
            return;

        ScheduleType = type;

        TimeOfDay = null;
        TimesOfDay.Clear();
        IntervalInHours = null;
        FirstDoseAt = null;
        WeekDays.Clear();
    }

    #endregion

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
