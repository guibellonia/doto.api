using Doto.Domain.Enums;

namespace Doto.Application.DTOs.Responses;

public record ScheduleReportItem
{
    public TimeOnly? ScheduledTime { get; init; }
    public MedicineScheduleType ScheduleType { get; init; }
    public List<WeekDay> WeekDays { get; init; } = new();
}

