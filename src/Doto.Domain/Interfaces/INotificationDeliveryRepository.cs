using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface INotificationDeliveryRepository
{
    Task<NotificationDelivery?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDelivery>> GetDueAsync(DateTime asOfUtc, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDelivery>> GetScheduledByOccurrenceAsync(Guid doseOccurrenceId, CancellationToken cancellationToken = default);

    Task AddAsync(NotificationDelivery delivery, CancellationToken cancellationToken = default);

    Task UpdateAsync(NotificationDelivery delivery, CancellationToken cancellationToken = default);
}
