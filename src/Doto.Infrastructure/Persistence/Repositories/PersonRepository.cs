using System.Linq;
using Microsoft.EntityFrameworkCore;
using Doto.Domain.Entities;
using Doto.Domain.Interfaces;
using Doto.Infrastructure.Persistence;

namespace Doto.Infrastructure.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly DotoDbContext _context;

    public PersonRepository(DotoDbContext context)
    {
        _context = context;
    }

    public async Task<Person?> GetBySupabaseUserIdAsync(string supabaseUserId)
    {
        return await _context.Persons.FirstOrDefaultAsync(p => p.SupabaseUserId == supabaseUserId);
    }

    public async Task<Person?> GetByIdAsync(Guid id)
    {
        return await _context.Persons.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Person person)
    {
        await _context.Persons.AddAsync(person);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Person>> GetMembersByOwnerIdAsync(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            return Enumerable.Empty<Person>();

        var members = await _context.Persons
            .AsNoTracking()
            .Where(p => p.Member == true && p.SupabaseUserSponsorId != null && p.SupabaseUserSponsorId == ownerId)
            .ToListAsync();

        return members;
    }
}
