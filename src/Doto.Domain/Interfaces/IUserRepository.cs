using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetAccessibleUserIdsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppUser>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    Task AddAsync(AppUser user, CancellationToken cancellationToken = default);

    Task UpdateAsync(AppUser user, CancellationToken cancellationToken = default);
}
