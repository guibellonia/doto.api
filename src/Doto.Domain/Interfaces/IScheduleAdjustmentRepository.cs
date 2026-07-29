using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IScheduleAdjustmentRepository
{
    Task<IReadOnlyList<ScheduleAdjustment>> GetByScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    Task AddAsync(ScheduleAdjustment adjustment, CancellationToken cancellationToken = default);
}
