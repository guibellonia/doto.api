namespace Doto.Application.DTOs.Responses;

public record VitalSignRecordDto(
    Guid Id,
    int Type,
    float Value,
    string? Unit,
    float? SecondaryValue,
    DateTime RecordedAt,
    string? Notes
);

