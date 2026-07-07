using Doto.Application.Interfaces;
using Doto.Domain.Entities;
using Doto.Domain.Interfaces;

namespace Doto.Application.Helpers
{
    public class PersonHelper
    {
        public static async Task<Person> GetCurrentPersonAsync(ICurrentUserService currentUser, IPersonRepository personRepository)
        {
            var supabaseUserId = currentUser.SupabaseUserId
                ?? throw new UnauthorizedAccessException("Invalid or expired token.");

            var person = await personRepository.GetBySupabaseUserIdAsync(supabaseUserId)
                ?? throw new InvalidOperationException("Person not found for current user.");

            return person;
        }
    }
}
