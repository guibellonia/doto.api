namespace Doto.Application.DTOs.Requests.Health;

public record RegisterWeightRequest(
    float WeightKg,
    DateTime RecordedAt,
    string? Notes = null
);

