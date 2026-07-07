using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface ISymptomRecordRepository
{
    Task<SymptomRecord?> GetByIdAsync(Guid id);
    Task AddAsync(SymptomRecord record);
    Task UpdateAsync(SymptomRecord record);
    Task DeleteAsync(SymptomRecord record);
    Task<IReadOnlyList<SymptomRecord>> GetByPersonIdAsync(Guid personId);
}

