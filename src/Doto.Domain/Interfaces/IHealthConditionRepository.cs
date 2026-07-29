using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IHealthConditionRepository
{
    Task<HealthCondition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HealthCondition>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(HealthCondition condition, CancellationToken cancellationToken = default);

    Task UpdateAsync(HealthCondition condition, CancellationToken cancellationToken = default);
}
