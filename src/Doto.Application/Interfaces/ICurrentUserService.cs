namespace Doto.Application.Interfaces;

public interface ICurrentUserService
{
    string? SupabaseUserId { get; }
    string? Email { get; }
}

