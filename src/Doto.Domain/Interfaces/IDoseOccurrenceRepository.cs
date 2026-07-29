using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IDoseOccurrenceRepository
{
    Task<DoseOccurrence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoseOccurrence>> GetByUsersAndDateRangeAsync(
        IReadOnlyCollection<Guid> userIds,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoseOccurrence>> GetPendingByScheduleAsync(
        Guid scheduleId,
        int generationVersion,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoseOccurrence>> GetOverduePendingAsync(DateTime asOfUtc, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<DoseOccurrence> occurrences, CancellationToken cancellationToken = default);

    Task UpdateAsync(DoseOccurrence occurrence, CancellationToken cancellationToken = default);
}
