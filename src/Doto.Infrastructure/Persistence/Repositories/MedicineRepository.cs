using Microsoft.EntityFrameworkCore;
using Doto.Domain.Entities;
using Doto.Domain.Interfaces;
using Doto.Infrastructure.Persistence;

namespace Doto.Infrastructure.Repositories;

public class MedicineRepository : IMedicineRepository
{
    private readonly DotoDbContext _context;

    public MedicineRepository(DotoDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Medicine medicine)
    {
        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<Medicine> Items, int TotalCount)> GetByPersonIdPagedAsync(Guid personId, int page, int pageSize)
    {
        var query = _context.Medicines
            .Where(m => m.PersonId == personId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<Medicine>> GetByPersonIdAsync(Guid personId)
    {
        return await _context.Medicines
            .Where(m => m.PersonId == personId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateAsync(Medicine medicine)
    {
        _context.Medicines.Update(medicine);
        await _context.SaveChangesAsync();
    }

    public async Task<Medicine?> GetByIdAsync(Guid id)
    {
        return await _context.Medicines
            .Where(m => !m.IsDeleted)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (medicine != null)
        {
            medicine.SoftDelete();
            await _context.SaveChangesAsync();
        }
    }
}
