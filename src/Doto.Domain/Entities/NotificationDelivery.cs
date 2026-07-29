using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class NotificationDelivery : EntityBase
{
    private NotificationDelivery()
    {
    }

    private NotificationDelivery(
        Guid recipientUserId,
        NotificationKind kind,
        DateTime scheduledForUtc,
        Guid? deviceId,
        Guid? notificationPreferenceId,
        Guid? doseOccurrenceId,
        string? payload)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (recipientUserId == Guid.Empty)
        {
            throw new ArgumentException("A entrega precisa de um destinatário.", nameof(recipientUserId));
        }

        RecipientUserId = recipientUserId;
        Kind = kind;
        ScheduledForUtc = scheduledForUtc;
        DeviceId = deviceId;
        NotificationPreferenceId = notificationPreferenceId;
        DoseOccurrenceId = doseOccurrenceId;
        Payload = payload;
        Status = DeliveryStatus.Scheduled;
        AttemptCount = 0;
    }

    public Guid RecipientUserId { get; private set; }

    public Guid? DeviceId { get; private set; }

    public Guid? NotificationPreferenceId { get; private set; }

    public Guid? DoseOccurrenceId { get; private set; }

    public NotificationKind Kind { get; private set; }

    public DateTime ScheduledForUtc { get; private set; }

    public DateTime? SentAtUtc { get; private set; }

    public DeliveryStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public string? ProviderMessageId { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string? Payload { get; private set; }

    public AppUser? RecipientUser { get; private set; }

    public Device? Device { get; private set; }

    public NotificationPreference? NotificationPreference { get; private set; }

    public DoseOccurrence? DoseOccurrence { get; private set; }

    public static NotificationDelivery Schedule(
        Guid recipientUserId,
        NotificationKind kind,
        DateTime scheduledForUtc,
        Guid? deviceId = null,
        Guid? notificationPreferenceId = null,
        Guid? doseOccurrenceId = null,
        string? payload = null)
        => new(recipientUserId, kind, scheduledForUtc, deviceId, notificationPreferenceId, doseOccurrenceId, payload);

    public void MarkSent(DateTime sentAtUtc, string? providerMessageId = null)
    {
        Status = DeliveryStatus.Sent;
        SentAtUtc = sentAtUtc;
        ProviderMessageId = providerMessageId;
        AttemptCount++;
        ErrorMessage = null;
    }

    public void MarkDelivered() => Status = DeliveryStatus.Delivered;

    public void MarkAcknowledged() => Status = DeliveryStatus.Acknowledged;

    public void MarkFailed(string errorMessage)
    {
        Status = DeliveryStatus.Failed;
        AttemptCount++;
        ErrorMessage = errorMessage;
    }

    public void Cancel()
    {
        if (Status == DeliveryStatus.Scheduled)
        {
            Status = DeliveryStatus.Cancelled;
        }
    }
}
