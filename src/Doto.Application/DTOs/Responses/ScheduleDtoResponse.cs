using Doto.Application.DTOs.Responses;
using Doto.Domain.Entities;
using Doto.Domain.Enums;

namespace Doto.Application.DTOs;

public record ScheduleDtoResponse(
    Guid Id,
    Guid MedicineId,
    MedicineScheduleType ScheduleType,
    TimeOnly? TimeOfDay,
    List<TimeOnly>? TimesOfDay,
    int? IntervalInHours,
    DateTimeOffset? FirstDoseAt,
    List<int>? WeekDays,
    int? PreAlarmMinutes,
    int? PosAlarmMinutes,
    List<DoseOccurrenceDto>? DoseOccurrences = null
)
{
    public static ScheduleDtoResponse FromEntity(Schedule s, List<DoseOccurrenceDto>? doseOccurrences = null)
    {
        return new ScheduleDtoResponse(
            Id: s.Id,
            MedicineId: s.MedicineId,
            ScheduleType: s.ScheduleType,
            TimeOfDay: s.TimeOfDay,
            TimesOfDay: s.TimesOfDay?.Select(t => t.Time).ToList(),
            IntervalInHours: s.IntervalInHours,
            FirstDoseAt: s.FirstDoseAt,
            WeekDays: s.WeekDays?.Select(w => w.DayOfWeek).ToList(),
            PreAlarmMinutes: s.PreAlarmMinutes,
            PosAlarmMinutes: s.PosAlarmMinutes,
            DoseOccurrences: doseOccurrences
        );
    }
}
