using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IScheduleRepository
{
    Task AddAsync(Schedule schedule);
    Task UpdateAsync(Schedule schedule);
    Task<Schedule?> GetByIdAsync(Guid id);
    Task<List<Schedule>> GetByMedicineIdAsync(Guid medicineId);
    Task<List<Schedule>> GetAllByMedicineIdAsync(Guid medicineId); // Includes inactive schedules
    Task<List<Schedule>> GetByPersonIdAsync(Guid personId);
    Task<(IReadOnlyList<Schedule> schedules, int total)> GetByPersonIdPagedAsync(Guid personId, int page, int pageSize);
}