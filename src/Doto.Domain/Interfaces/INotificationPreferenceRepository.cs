using Doto.Domain.Entities;
using Doto.Domain.Enums;

namespace Doto.Domain.Interfaces;

public interface INotificationPreferenceRepository
{
    Task<NotificationPreference?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationPreference>> GetBySubjectAsync(Guid subjectUserId, CancellationToken cancellationToken = default);

    Task<NotificationPreference?> GetEffectiveAsync(
        Guid subjectUserId,
        Guid recipientUserId,
        NotificationKind kind,
        Guid? medicationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(NotificationPreference preference, CancellationToken cancellationToken = default);

    Task UpdateAsync(NotificationPreference preference, CancellationToken cancellationToken = default);
}
