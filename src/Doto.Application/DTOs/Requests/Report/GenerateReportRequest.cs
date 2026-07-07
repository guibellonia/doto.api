using Doto.Domain.Enums;

namespace Doto.Application.DTOs.Requests.Report;

public record GenerateReportRequest
{
    public List<ReportType> ReportTypes { get; init; } = new();
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}

