namespace Doto.Domain.Entities;

public class ScheduleWeekDay
{
    // Id removido - chave primária agora é composta (ScheduleId, DayOfWeek)
    public Guid ScheduleId { get; private set; }
    public Schedule Schedule { get; private set; } = null!;

    public int DayOfWeek { get; private set; }

    protected ScheduleWeekDay() { }

    public ScheduleWeekDay(Guid scheduleId, int dayOfWeek)
    {
        if (dayOfWeek < 1 || dayOfWeek > 7)
            throw new ArgumentOutOfRangeException(nameof(dayOfWeek), "DayOfWeek must be between 1 and 7.");

        ScheduleId = scheduleId;
        DayOfWeek = dayOfWeek;
    }
}