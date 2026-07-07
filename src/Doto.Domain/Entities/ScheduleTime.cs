namespace Doto.Domain.Entities
{
    public class ScheduleTime
    {
        public Guid Id { get; private set; }
        public Guid ScheduleId { get; private set; }
        public Schedule Schedule { get; private set; } = null!;

        public TimeOnly Time { get; private set; }

        protected ScheduleTime() { }

        public ScheduleTime(Guid id, Guid scheduleId, TimeOnly time)
        {
            Id = id;
            ScheduleId = scheduleId;
            Time = time;
        }
    }
}
