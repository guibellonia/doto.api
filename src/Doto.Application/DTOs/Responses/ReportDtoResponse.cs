using Doto.Domain.Enums;

namespace Doto.Application.DTOs.Responses;

public record ReportDtoResponse
{
    public ReportType Type { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public MedicinesReportData? MedicinesData { get; init; }
    public VitalSignsReportData? VitalSignsData { get; init; }
    public SymptomsReportData? SymptomsData { get; init; }
    public CompleteReportData? CompleteData { get; init; }
}

