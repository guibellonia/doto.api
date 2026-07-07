using Microsoft.EntityFrameworkCore;
using Doto.Domain.Entities;
using Doto.Domain.Interfaces;
using Doto.Infrastructure.Persistence;

namespace Doto.Infrastructure.Repositories;

public class VitalSignRecordRepository : IVitalSignRecordRepository
{
    private readonly DotoDbContext _context;

    public VitalSignRecordRepository(DotoDbContext context)
    {
        _context = context;
    }

    public async Task<VitalSignRecord?> GetByIdAsync(Guid id)
    {
        return await _context.VitalSignRecords
            .Include(v => v.Person)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task AddAsync(VitalSignRecord record)
    {
        await _context.VitalSignRecords.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(VitalSignRecord record)
    {
        _context.VitalSignRecords.Update(record);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(VitalSignRecord record)
    {
        _context.VitalSignRecords.Remove(record);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<VitalSignRecord>> GetByPersonIdAsync(Guid personId)
    {
        return await _context.VitalSignRecords
            .Where(v => v.PersonId == personId)
            .OrderByDescending(v => v.RecordedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<VitalSignRecord>> GetByPersonIdAndTypeAsync(Guid personId, int type)
    {
        return await _context.VitalSignRecords
            .Where(v => v.PersonId == personId && (int)v.Type == type)
            .OrderByDescending(v => v.RecordedAt)
            .ToListAsync();
    }

    public async Task<VitalSignRecord?> GetLatestByPersonIdAndTypeAsync(Guid personId, int type)
    {
        return await _context.VitalSignRecords
            .Where(v => v.PersonId == personId && (int)v.Type == type)
            .OrderByDescending(v => v.RecordedAt)
            .FirstOrDefaultAsync();
    }
}

