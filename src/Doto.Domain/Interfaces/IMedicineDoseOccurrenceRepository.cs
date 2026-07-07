using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IMedicineDoseOccurrenceRepository
{
    Task<MedicineDoseOccurrence?> GetByIdAsync(Guid id);
    Task AddAsync(MedicineDoseOccurrence occurrence);
    Task AddRangeAsync(IEnumerable<MedicineDoseOccurrence> occurrences);
    Task UpdateAsync(MedicineDoseOccurrence occurrence);

    Task DeleteFuturePendingByMedicineAsync(Guid medicineId, DateTimeOffset from);

    Task<IReadOnlyList<MedicineDoseOccurrence>> GetByPersonAndPeriodAsync(Guid personId, DateTimeOffset from, DateTimeOffset to);
    
    Task<MedicineDoseOccurrence?> GetByMedicineScheduleAndDateAsync(Guid medicineId, Guid scheduleId, DateTimeOffset scheduledAt);

    Task<IReadOnlyList<MedicineDoseOccurrence>> GetPendingDosesInTimeRangeAsync(DateTimeOffset from, DateTimeOffset to);
    
    Task<IReadOnlyList<MedicineDoseOccurrence>> GetByScheduleIdsAndDateAsync(IEnumerable<Guid> scheduleIds, DateOnly date);
    
    Task<bool> HasTakenDosesAsync(Guid scheduleId);
}
