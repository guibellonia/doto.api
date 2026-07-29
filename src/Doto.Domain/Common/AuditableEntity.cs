namespace Doto.Domain.Common;

public abstract class AuditableEntity : EntityBase
{
    protected AuditableEntity()
    {
    }

    protected AuditableEntity(Guid id, DateTime createdAtUtc)
        : base(id, createdAtUtc)
    {
    }

    public DateTime? UpdatedAt { get; protected set; }

    protected void Touch() => UpdatedAt = DateTime.UtcNow;
}
