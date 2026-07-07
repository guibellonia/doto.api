using Doto.Domain.Enums;

namespace Doto.Application.DTOs.Responses;

public record MedicineReportItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public float DosageValue { get; init; }
    public DosageUnit DosageUnit { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Observations { get; init; }
    public List<ScheduleReportItem> Schedules { get; init; } = new();
}

