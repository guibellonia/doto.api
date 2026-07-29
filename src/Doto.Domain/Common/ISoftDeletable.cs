namespace Doto.Domain.Common;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; }
}
