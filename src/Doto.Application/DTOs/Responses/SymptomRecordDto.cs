namespace Doto.Application.DTOs.Responses;

public record SymptomRecordDto(
    Guid Id,
    string Symptoms,
    int? Severity,
    DateTime RecordedAt,
    string? Notes
);

