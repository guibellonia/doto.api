using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class ChildInvite : EntityBase
{
    private ChildInvite()
    {
        InvitedEmail = null!;
    }

    private ChildInvite(
        Guid parentUserId,
        Guid childUserId,
        string invitedEmail,
        DateTime expiresAtUtc,
        string? inviteTokenHash)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (parentUserId == Guid.Empty)
        {
            throw new ArgumentException("O convite precisa de um cuidador.", nameof(parentUserId));
        }

        if (childUserId == Guid.Empty)
        {
            throw new ArgumentException("O convite precisa da conta do dependente já criada.", nameof(childUserId));
        }

        if (parentUserId == childUserId)
        {
            throw new ArgumentException("Um usuário não pode convidar a si mesmo.", nameof(childUserId));
        }

        InvitedEmail = string.IsNullOrWhiteSpace(invitedEmail)
            ? throw new ArgumentException("O email convidado é obrigatório.", nameof(invitedEmail))
            : invitedEmail.Trim().ToLowerInvariant();

        ParentUserId = parentUserId;
        ChildUserId = childUserId;
        ExpiresAtUtc = expiresAtUtc;
        InviteTokenHash = inviteTokenHash;
        Status = InviteStatus.Pending;
        ResentCount = 0;
    }

    public Guid ParentUserId { get; private set; }

    public Guid ChildUserId { get; private set; }

    public string InvitedEmail { get; private set; }

    public InviteStatus Status { get; private set; }

    public string? InviteTokenHash { get; private set; }

    public DateTime? TempPasswordSentAt { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? AcceptedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public int ResentCount { get; private set; }

    public DateTime? LastSentAtUtc { get; private set; }

    public AppUser? ParentUser { get; private set; }

    public AppUser? ChildUser { get; private set; }

    public static ChildInvite Create(
        Guid parentUserId,
        Guid childUserId,
        string invitedEmail,
        DateTime expiresAtUtc,
        string? inviteTokenHash = null)
        => new(parentUserId, childUserId, invitedEmail, expiresAtUtc, inviteTokenHash);

    public void MarkSent(DateTime sentAtUtc)
    {
        TempPasswordSentAt ??= sentAtUtc;
        LastSentAtUtc = sentAtUtc;
    }

    public void Resend(DateTime sentAtUtc, DateTime newExpiresAtUtc)
    {
        EnsurePending();
        ResentCount++;
        LastSentAtUtc = sentAtUtc;
        TempPasswordSentAt = sentAtUtc;
        ExpiresAtUtc = newExpiresAtUtc;
    }

    public void Accept(DateTime acceptedAtUtc)
    {
        EnsurePending();
        Status = InviteStatus.Accepted;
        AcceptedAtUtc = acceptedAtUtc;
    }

    public void Revoke(DateTime revokedAtUtc)
    {
        EnsurePending();
        Status = InviteStatus.Revoked;
        RevokedAtUtc = revokedAtUtc;
    }

    public void Expire()
    {
        if (Status == InviteStatus.Pending)
        {
            Status = InviteStatus.Expired;
        }
    }

    public bool IsExpiredAt(DateTime nowUtc) => Status == InviteStatus.Pending && ExpiresAtUtc <= nowUtc;

    private void EnsurePending()
    {
        if (Status != InviteStatus.Pending)
        {
            throw new InvalidOperationException($"O convite não está pendente (status atual: {Status}).");
        }
    }
}
