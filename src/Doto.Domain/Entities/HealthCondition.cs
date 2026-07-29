using Doto.Domain.Common;

namespace Doto.Domain.Entities;

public class HealthCondition : EntityBase, ISoftDeletable
{
    private HealthCondition()
    {
        Name = null!;
    }

    private HealthCondition(Guid userId, string name, string? notes, DateOnly? diagnosedOn)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A condição precisa pertencer a um usuário.", nameof(userId));
        }

        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("O nome da condição é obrigatório.", nameof(name))
            : name.Trim();

        UserId = userId;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        DiagnosedOn = diagnosedOn;
        IsActive = true;
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public string? Notes { get; private set; }

    public DateOnly? DiagnosedOn { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public AppUser? User { get; private set; }

    public static HealthCondition Declare(Guid userId, string name, string? notes = null, DateOnly? diagnosedOn = null)
        => new(userId, name, notes, diagnosedOn);

    public void Update(string name, string? notes, DateOnly? diagnosedOn)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("O nome da condição é obrigatório.", nameof(name))
            : name.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        DiagnosedOn = diagnosedOn;
    }

    public void Resolve() => IsActive = false;

    public void Reopen() => IsActive = true;

    public void SoftDelete(DateTime deletedAtUtc)
    {
        DeletedAt ??= deletedAtUtc;
        IsActive = false;
    }
}
