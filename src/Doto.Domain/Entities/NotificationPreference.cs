using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class NotificationPreference : AuditableEntity, ISoftDeletable
{
    private NotificationPreference()
    {
    }

    private NotificationPreference(
        Guid subjectUserId,
        Guid recipientUserId,
        Guid? medicationId,
        NotificationKind kind,
        short leadMinutes,
        NotificationChannel channel)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (subjectUserId == Guid.Empty)
        {
            throw new ArgumentException("A preferência precisa de um sujeito.", nameof(subjectUserId));
        }

        if (recipientUserId == Guid.Empty)
        {
            throw new ArgumentException("A preferência precisa de um destinatário.", nameof(recipientUserId));
        }

        if (leadMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(leadMinutes), "A antecedência não pode ser negativa.");
        }

        SubjectUserId = subjectUserId;
        RecipientUserId = recipientUserId;
        MedicationId = medicationId;
        Kind = kind;
        LeadMinutes = leadMinutes;
        Channel = channel;
        Enabled = true;
    }

    public Guid SubjectUserId { get; private set; }

    public Guid RecipientUserId { get; private set; }

    public Guid? MedicationId { get; private set; }

    public NotificationKind Kind { get; private set; }

    public bool Enabled { get; private set; }

    public short LeadMinutes { get; private set; }

    public short? RepeatIntervalMinutes { get; private set; }

    public short? MaxRepeats { get; private set; }

    public TimeOnly? QuietHoursStart { get; private set; }

    public TimeOnly? QuietHoursEnd { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public AppUser? SubjectUser { get; private set; }

    public AppUser? RecipientUser { get; private set; }

    public Medication? Medication { get; private set; }

    public static NotificationPreference ForSelf(
        Guid userId,
        NotificationKind kind,
        short leadMinutes,
        NotificationChannel channel = NotificationChannel.Both,
        Guid? medicationId = null)
        => new(userId, userId, medicationId, kind, leadMinutes, channel);

    public static NotificationPreference ForCaregiver(
        Guid subjectUserId,
        Guid recipientUserId,
        NotificationKind kind,
        short leadMinutes,
        NotificationChannel channel = NotificationChannel.Both,
        Guid? medicationId = null)
        => new(subjectUserId, recipientUserId, medicationId, kind, leadMinutes, channel);

    public void Configure(
        short leadMinutes,
        NotificationChannel channel,
        short? repeatIntervalMinutes = null,
        short? maxRepeats = null)
    {
        if (leadMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(leadMinutes), "A antecedência não pode ser negativa.");
        }

        if (repeatIntervalMinutes is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(repeatIntervalMinutes), "O intervalo de repetição deve ser positivo.");
        }

        if (maxRepeats is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRepeats), "O máximo de repetições não pode ser negativo.");
        }

        LeadMinutes = leadMinutes;
        Channel = channel;
        RepeatIntervalMinutes = repeatIntervalMinutes;
        MaxRepeats = maxRepeats;
        Touch();
    }

    public void SetQuietHours(TimeOnly? start, TimeOnly? end)
    {
        if (start is null != end is null)
        {
            throw new ArgumentException("A janela de silêncio exige início e fim, ou nenhum dos dois.");
        }

        QuietHoursStart = start;
        QuietHoursEnd = end;
        Touch();
    }

    public void Enable()
    {
        Enabled = true;
        Touch();
    }

    public void Disable()
    {
        Enabled = false;
        Touch();
    }

    public void SoftDelete(DateTime deletedAtUtc)
    {
        if (DeletedAt is not null)
        {
            return;
        }

        DeletedAt = deletedAtUtc;
        Enabled = false;
        Touch();
    }
}
