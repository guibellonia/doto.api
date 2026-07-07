namespace Doto.Application.DTOs.Requests.Health;

public record RegisterSymptomRequest(
    string Symptoms,
    DateTime RecordedAt,
    int? Severity = null,
    string? Notes = null
);

