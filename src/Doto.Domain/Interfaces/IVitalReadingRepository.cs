using Doto.Domain.Entities;
using Doto.Domain.Enums;

namespace Doto.Domain.Interfaces;

public interface IVitalReadingRepository
{
    Task<VitalReading?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VitalReading>> GetByUsersAndDateRangeAsync(
        IReadOnlyCollection<Guid> userIds,
        DateOnly from,
        DateOnly to,
        VitalType? type = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(VitalReading reading, CancellationToken cancellationToken = default);

    Task UpdateAsync(VitalReading reading, CancellationToken cancellationToken = default);
}
