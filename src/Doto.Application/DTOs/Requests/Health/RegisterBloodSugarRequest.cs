namespace Doto.Application.DTOs.Requests.Health;

public record RegisterBloodSugarRequest(
    float Value,
    DateTime RecordedAt,
    string? Unit = null,
    string? Notes = null
);

