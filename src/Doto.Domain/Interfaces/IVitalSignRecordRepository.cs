using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IVitalSignRecordRepository
{
    Task<VitalSignRecord?> GetByIdAsync(Guid id);
    Task AddAsync(VitalSignRecord record);
    Task UpdateAsync(VitalSignRecord record);
    Task DeleteAsync(VitalSignRecord record);
    Task<IReadOnlyList<VitalSignRecord>> GetByPersonIdAsync(Guid personId);
    Task<IReadOnlyList<VitalSignRecord>> GetByPersonIdAndTypeAsync(Guid personId, int type);
    Task<VitalSignRecord?> GetLatestByPersonIdAndTypeAsync(Guid personId, int type);
}

