using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class AppUser : AuditableEntity, ISoftDeletable
{
    public const string DefaultTimeZone = "America/Sao_Paulo";

    private readonly List<AppUser> _children = [];
    private readonly List<HealthCondition> _healthConditions = [];

    private AppUser()
    {
        Email = null!;
        Username = null!;
        DisplayName = null!;
        TimeZone = DefaultTimeZone;
    }

    private AppUser(
        Guid authUserId,
        string email,
        string username,
        string displayName,
        UserRole role,
        UserStatus status,
        Guid? parentId,
        bool mustChangePassword,
        string timeZone)
        : base(authUserId, DateTime.UtcNow)
    {
        if (authUserId == Guid.Empty)
        {
            throw new ArgumentException("O id do usuário deve vir do auth.users do Supabase.", nameof(authUserId));
        }

        Email = NormalizeEmail(email);
        Username = NormalizeUsername(username);
        DisplayName = Require(displayName, nameof(displayName));
        Role = role;
        Status = status;
        ParentId = parentId;
        MustChangePassword = mustChangePassword;
        TimeZone = string.IsNullOrWhiteSpace(timeZone) ? DefaultTimeZone : timeZone.Trim();
    }

    public Guid? ParentId { get; private set; }

    public string Email { get; private set; }

    public string Username { get; private set; }

    public string DisplayName { get; private set; }

    public UserRole Role { get; private set; }

    public UserStatus Status { get; private set; }

    public bool MustChangePassword { get; private set; }

    public DateTime? PasswordChangedAt { get; private set; }

    public DateOnly? BirthDate { get; private set; }

    public decimal? WeightKg { get; private set; }

    public decimal? HeightCm { get; private set; }

    public string? AvatarUrl { get; private set; }

    public string TimeZone { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public AppUser? Parent { get; private set; }

    public IReadOnlyCollection<AppUser> Children => _children;

    public IReadOnlyCollection<HealthCondition> HealthConditions => _healthConditions;

    public static AppUser RegisterParent(
        Guid authUserId,
        string email,
        string username,
        string displayName,
        string? timeZone = null)
        => new(
            authUserId,
            email,
            username,
            displayName,
            UserRole.Parent,
            UserStatus.Active,
            parentId: null,
            mustChangePassword: false,
            timeZone ?? DefaultTimeZone);

    public static AppUser InviteChild(
        Guid authUserId,
        Guid parentId,
        string email,
        string username,
        string displayName,
        string? timeZone = null)
    {
        if (parentId == Guid.Empty)
        {
            throw new ArgumentException("Um dependente precisa de um cuidador responsável.", nameof(parentId));
        }

        if (parentId == authUserId)
        {
            throw new ArgumentException("Um usuário não pode ser cuidador de si mesmo.", nameof(parentId));
        }

        return new AppUser(
            authUserId,
            email,
            username,
            displayName,
            UserRole.Child,
            UserStatus.PendingActivation,
            parentId,
            mustChangePassword: true,
            timeZone ?? DefaultTimeZone);
    }

    public void UpdateProfile(string displayName, DateOnly? birthDate, string? avatarUrl)
    {
        DisplayName = Require(displayName, nameof(displayName));
        BirthDate = birthDate;
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        Touch();
    }

    public void UpdateBodyMetrics(decimal? weightKg, decimal? heightCm)
    {
        if (weightKg is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weightKg), "O peso deve ser positivo.");
        }

        if (heightCm is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heightCm), "A altura deve ser positiva.");
        }

        WeightKg = weightKg;
        HeightCm = heightCm;
        Touch();
    }

    public void ChangeTimeZone(string timeZone)
    {
        TimeZone = string.IsNullOrWhiteSpace(timeZone)
            ? throw new ArgumentException("O fuso horário é obrigatório.", nameof(timeZone))
            : timeZone.Trim();
        Touch();
    }

    public void SyncEmailFromAuthProvider(string email)
    {
        var normalized = NormalizeEmail(email);
        if (normalized == Email)
        {
            return;
        }

        Email = normalized;
        Touch();
    }

    public void CompletePasswordChange(DateTime changedAtUtc)
    {
        MustChangePassword = false;
        PasswordChangedAt = changedAtUtc;

        if (Status == UserStatus.PendingActivation)
        {
            Status = UserStatus.Active;
        }

        Touch();
    }

    public void Disable()
    {
        Status = UserStatus.Disabled;
        Touch();
    }

    public void Reactivate()
    {
        if (Status == UserStatus.Disabled)
        {
            Status = UserStatus.Active;
            Touch();
        }
    }

    public void SoftDelete(DateTime deletedAtUtc)
    {
        if (DeletedAt is not null)
        {
            return;
        }

        DeletedAt = deletedAtUtc;
        Status = UserStatus.Disabled;
        Touch();
    }

    private static string NormalizeEmail(string email)
        => string.IsNullOrWhiteSpace(email)
            ? throw new ArgumentException("O email é obrigatório.", nameof(email))
            : email.Trim().ToLowerInvariant();

    private static string NormalizeUsername(string username)
        => string.IsNullOrWhiteSpace(username)
            ? throw new ArgumentException("O nome de usuário é obrigatório.", nameof(username))
            : username.Trim().ToLowerInvariant();

    private static string Require(string value, string paramName)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"'{paramName}' é obrigatório.", paramName)
            : value.Trim();
}
