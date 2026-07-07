namespace Doto.Application.DTOs.Requests.Health;

public record RegisterHeightRequest(
    int HeightCm,
    DateTime RecordedAt,
    string? Notes = null
);

