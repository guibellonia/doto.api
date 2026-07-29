namespace Doto.Domain.Common;

public abstract class EntityBase
{
    protected EntityBase()
    {
    }

    protected EntityBase(Guid id, DateTime createdAtUtc)
    {
        Id = id;
        CreatedAt = createdAtUtc;
    }

    public Guid Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; }
}
