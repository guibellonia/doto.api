using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IMedicationScheduleRepository
{
    Task<MedicationSchedule?> GetByIdWithTimeSlotsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MedicationSchedule>> GetByMedicationAsync(Guid medicationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MedicationSchedule>> GetPendingGenerationAsync(DateOnly throughDate, CancellationToken cancellationToken = default);

    Task AddAsync(MedicationSchedule schedule, CancellationToken cancellationToken = default);

    Task UpdateAsync(MedicationSchedule schedule, CancellationToken cancellationToken = default);
}
