using Doto.Domain.Enums;

namespace Doto.Application.DTOs.Requests.Schedule;

public class UpdateScheduleRequest
{
    public Guid Id { get; set; }
    public Guid MedicineId { get; set; }
    public MedicineScheduleType ScheduleType { get; set; }

    // OncePerDay e SpecificWeekDays
    public TimeOnly? TimeOfDay { get; set; }

    // MultipleFixedTimesPerDay
    public List<TimeOnly>? TimesOfDay { get; set; }

    // EveryXHours
    public int? IntervalInHours { get; set; }
    public DateTimeOffset? FirstDoseAt { get; set; }

    // SpecificWeekDays
    public List<int>? WeekDays { get; set; }

    public int? PreAlarmMinutes { get; set; }
    public int? PosAlarmMinutes { get; set; }
}