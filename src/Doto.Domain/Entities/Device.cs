using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class Device : EntityBase, ISoftDeletable
{
    private Device()
    {
        PushToken = null!;
    }

    private Device(
        Guid userId,
        string pushToken,
        DevicePlatform platform,
        string? deviceName,
        string? appVersion)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("O aparelho precisa pertencer a um usuário.", nameof(userId));
        }

        PushToken = string.IsNullOrWhiteSpace(pushToken)
            ? throw new ArgumentException("O token de push é obrigatório.", nameof(pushToken))
            : pushToken.Trim();

        UserId = userId;
        Platform = platform;
        DeviceName = string.IsNullOrWhiteSpace(deviceName) ? null : deviceName.Trim();
        AppVersion = string.IsNullOrWhiteSpace(appVersion) ? null : appVersion.Trim();
        IsActive = true;
        LastSeenAtUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }

    public string PushToken { get; private set; }

    public DevicePlatform Platform { get; private set; }

    public string? DeviceName { get; private set; }

    public string? AppVersion { get; private set; }

    public DateTime? LastSeenAtUtc { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public AppUser? User { get; private set; }

    public static Device Register(
        Guid userId,
        string pushToken,
        DevicePlatform platform,
        string? deviceName = null,
        string? appVersion = null)
        => new(userId, pushToken, platform, deviceName, appVersion);

    public void ReassignTo(Guid userId, DevicePlatform platform, string? deviceName, string? appVersion)
    {
        UserId = userId;
        Platform = platform;
        DeviceName = string.IsNullOrWhiteSpace(deviceName) ? null : deviceName.Trim();
        AppVersion = string.IsNullOrWhiteSpace(appVersion) ? null : appVersion.Trim();
        IsActive = true;
        DeletedAt = null;
        LastSeenAtUtc = DateTime.UtcNow;
    }

    public void Heartbeat(DateTime seenAtUtc, string? appVersion = null)
    {
        LastSeenAtUtc = seenAtUtc;
        if (!string.IsNullOrWhiteSpace(appVersion))
        {
            AppVersion = appVersion.Trim();
        }
    }

    public void Deactivate() => IsActive = false;

    public void SoftDelete(DateTime deletedAtUtc)
    {
        DeletedAt ??= deletedAtUtc;
        IsActive = false;
    }
}
