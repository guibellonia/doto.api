namespace Doto.Application.DTOs.Requests;

public record UpdatePersonRequest(
    string? Name = null,
    string? Phone = null,
    string? Email = null,
    float? WeightKg = null,
    int? HeightCm = null
);

