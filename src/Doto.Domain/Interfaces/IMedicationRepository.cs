using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IMedicationRepository
{
    Task<Medication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Medication>> GetByUsersAsync(
        IReadOnlyCollection<Guid> userIds,
        bool onlyActive = true,
        CancellationToken cancellationToken = default);

    Task AddAsync(Medication medication, CancellationToken cancellationToken = default);

    Task UpdateAsync(Medication medication, CancellationToken cancellationToken = default);
}
