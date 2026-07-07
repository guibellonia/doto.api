using Doto.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doto.Domain.Interfaces;

public interface IPersonRepository
{
    Task<Person?> GetBySupabaseUserIdAsync(string supabaseUserId);
    Task<Person?> GetByIdAsync(Guid id);
    Task AddAsync(Person person);
    Task SaveChangesAsync();
    Task<IEnumerable<Person>> GetMembersByOwnerIdAsync(string ownerId);
}
