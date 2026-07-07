using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IMedicineRepository
{
    Task AddAsync(Medicine medicine);
    Task<(List<Medicine> Items, int TotalCount)> GetByPersonIdPagedAsync(Guid personId, int page, int pageSize);
    Task<List<Medicine>> GetByPersonIdAsync(Guid personId);
    Task UpdateAsync(Medicine medicine);
    Task<Medicine?> GetByIdAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
}
