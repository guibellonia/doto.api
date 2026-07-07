using Microsoft.EntityFrameworkCore;
using Doto.Domain.Entities;
using Doto.Domain.Interfaces;
using Doto.Infrastructure.Persistence;

namespace Doto.Infrastructure.Repositories;

public class SymptomRecordRepository : ISymptomRecordRepository
{
    private readonly DotoDbContext _context;

    public SymptomRecordRepository(DotoDbContext context)
    {
        _context = context;
    }

    public async Task<SymptomRecord?> GetByIdAsync(Guid id)
    {
        return await _context.SymptomRecords
            .Include(s => s.Person)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(SymptomRecord record)
    {
        await _context.SymptomRecords.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SymptomRecord record)
    {
        _context.SymptomRecords.Update(record);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(SymptomRecord record)
    {
        _context.SymptomRecords.Remove(record);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<SymptomRecord>> GetByPersonIdAsync(Guid personId)
    {
        return await _context.SymptomRecords
            .Where(s => s.PersonId == personId)
            .OrderByDescending(s => s.RecordedAt)
            .ToListAsync();
    }
}

