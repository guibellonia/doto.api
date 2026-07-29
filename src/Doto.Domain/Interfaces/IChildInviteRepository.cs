using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IChildInviteRepository
{
    Task<ChildInvite?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ChildInvite?> GetPendingByEmailAsync(Guid parentUserId, string invitedEmail, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChildInvite>> GetByParentAsync(Guid parentUserId, CancellationToken cancellationToken = default);

    Task AddAsync(ChildInvite invite, CancellationToken cancellationToken = default);

    Task UpdateAsync(ChildInvite invite, CancellationToken cancellationToken = default);
}
