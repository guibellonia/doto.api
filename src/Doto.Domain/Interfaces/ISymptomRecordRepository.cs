using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface ISymptomRecordRepository
{
    Task<SymptomRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SymptomRecord>> GetByUsersAndDateRangeAsync(
        IReadOnlyCollection<Guid> userIds,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task AddAsync(SymptomRecord record, CancellationToken cancellationToken = default);

    Task UpdateAsync(SymptomRecord record, CancellationToken cancellationToken = default);
}
