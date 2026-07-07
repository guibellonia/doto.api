namespace Doto.Application.DTOs.Requests.Health;

public record RegisterBloodPressureRequest(
    float SystolicValue,
    float DiastolicValue,
    DateTime RecordedAt,
    string? Notes = null
);

