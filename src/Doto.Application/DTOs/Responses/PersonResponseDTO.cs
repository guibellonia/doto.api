namespace Doto.Application.DTOs.Responses
{
    public record PersonResponseDTO(
        Guid Id, 
        string Name, 
        string Email,
        string Username,
        DateTime? BirthDate,
        string? Phone,
        float? WeightKg,
        int? HeightCm,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string? SupabaseUserId
    );
}
