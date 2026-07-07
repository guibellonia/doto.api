using Microsoft.EntityFrameworkCore;
using Doto.Domain.Entities;
using Doto.Domain.Interfaces;

namespace Doto.Infrastructure.Persistence;

public class ScheduleRepository : IScheduleRepository
{
    private readonly DotoDbContext _context;

    public ScheduleRepository(DotoDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Schedule schedule)
    {
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Schedule schedule)
    {
        _context.Schedules.Update(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task<Schedule?> GetByIdAsync(Guid id)
    {
        return await _context.Schedules
            .Include(s => s.Medicine)
            .Include(s => s.TimesOfDay)
            .Include(s => s.WeekDays)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Schedule>> GetByMedicineIdAsync(Guid medicineId)
    {
        return await _context.Schedules
            .Include(s => s.TimesOfDay)
            .Include(s => s.WeekDays)
            .Where(s => s.MedicineId == medicineId && s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetAllByMedicineIdAsync(Guid medicineId)
    {
        return await _context.Schedules
            .Include(s => s.TimesOfDay)
            .Include(s => s.WeekDays)
            .Where(s => s.MedicineId == medicineId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetByPersonIdAsync(Guid personId)
    {
        return await _context.Schedules
            .Include(s => s.TimesOfDay)
            .Include(s => s.WeekDays)
            .Include(s => s.Medicine)
            .Where(s => s.Medicine != null && s.Medicine.PersonId == personId && s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Schedule> schedules, int total)> GetByPersonIdPagedAsync(Guid personId, int page, int pageSize)
    {
        var query = _context.Schedules
            .Include(s => s.WeekDays)
            .Include(s => s.Medicine)
            .Where(s => s.Medicine != null && s.Medicine.PersonId == personId)
            .OrderByDescending(s => s.CreatedAt);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}