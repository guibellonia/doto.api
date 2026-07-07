using Microsoft.EntityFrameworkCore;
using Doto.Domain.Entities;
using Doto.Domain.Enums;
using Doto.Domain.Interfaces;
using Doto.Infrastructure.Persistence;

namespace Doto.Infrastructure.Repositories;

public class MedicineDoseOccurrenceRepository : IMedicineDoseOccurrenceRepository
{
    private readonly DotoDbContext _context;

    public MedicineDoseOccurrenceRepository(DotoDbContext context)
    {
        _context = context;
    }

    public async Task<MedicineDoseOccurrence?> GetByIdAsync(Guid id)
    {
        return await _context.MedicineDoseOccurrences
            .Include(d => d.Medicine)
            .Include(d => d.Schedule)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task AddAsync(MedicineDoseOccurrence occurrence)
    {
        await _context.MedicineDoseOccurrences.AddAsync(occurrence);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<MedicineDoseOccurrence> occurrences)
    {
        await _context.MedicineDoseOccurrences.AddRangeAsync(occurrences);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MedicineDoseOccurrence occurrence)
    {
        _context.MedicineDoseOccurrences.Update(occurrence);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFuturePendingByMedicineAsync(Guid medicineId, DateTimeOffset from)
    {
        var toDelete = await _context.MedicineDoseOccurrences
            .Where(d =>
                d.MedicineId == medicineId &&
                d.ScheduledAt >= from &&
                d.Status == DoseStatus.Pending)
            .ToListAsync();

        if (!toDelete.Any())
            return;

        _context.MedicineDoseOccurrences.RemoveRange(toDelete);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<MedicineDoseOccurrence>> GetByPersonAndPeriodAsync(
        Guid personId,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var query = _context.MedicineDoseOccurrences
            .Include(d => d.Medicine)
            .Include(d => d.Schedule)
            .Where(d =>
                d.Medicine.PersonId == personId &&
                d.ScheduledAt >= from &&
                d.ScheduledAt <= to);

        var items = await query
            .OrderBy(d => d.ScheduledAt)
            .ToListAsync();

        return items;
    }

    public async Task<MedicineDoseOccurrence?> GetByMedicineScheduleAndDateAsync(
        Guid medicineId,
        Guid scheduleId,
        DateTimeOffset scheduledAt)
    {
        var startOfDay = new DateTimeOffset(scheduledAt.Date, TimeSpan.Zero);
        var endOfDay = startOfDay.AddDays(1);

        return await _context.MedicineDoseOccurrences
            .Include(d => d.Medicine)
            .Include(d => d.Schedule)
            .FirstOrDefaultAsync(d =>
                d.MedicineId == medicineId &&
                d.ScheduleId == scheduleId &&
                d.ScheduledAt >= startOfDay &&
                d.ScheduledAt < endOfDay);
    }

    public async Task<IReadOnlyList<MedicineDoseOccurrence>> GetPendingDosesInTimeRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to)
    {
        return await _context.MedicineDoseOccurrences
            .Include(d => d.Medicine)
            .Include(d => d.Schedule)
            .ThenInclude(s => s!.TimesOfDay)
            .Where(d =>
                d.Status == DoseStatus.Pending &&
                d.ScheduledAt >= from &&
                d.ScheduledAt <= to)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<MedicineDoseOccurrence>> GetByScheduleIdsAndDateAsync(
        IEnumerable<Guid> scheduleIds,
        DateOnly date)
    {
        var scheduleIdsList = scheduleIds.ToList();
        if (!scheduleIdsList.Any())
            return Array.Empty<MedicineDoseOccurrence>();

        var startOfDay = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endOfDay = new DateTimeOffset(date.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        return await _context.MedicineDoseOccurrences
            .Include(d => d.Medicine)
            .Include(d => d.Schedule)
            .Where(d =>
                scheduleIdsList.Contains(d.ScheduleId) &&
                d.ScheduledAt >= startOfDay &&
                d.ScheduledAt < endOfDay)
            .OrderBy(d => d.ScheduledAt)
            .ToListAsync();
    }

    public async Task<bool> HasTakenDosesAsync(Guid scheduleId)
    {
        return await _context.MedicineDoseOccurrences
            .AnyAsync(d => d.ScheduleId == scheduleId && d.TakenAt != null);
    }
}
