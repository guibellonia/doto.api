namespace Doto.Application.DTOs.Responses;

public record CompleteReportData
{
    public MedicinesReportData Medicines { get; init; } = null!;
    public VitalSignsReportData VitalSigns { get; init; } = null!;
    public SymptomsReportData? Symptoms { get; init; }
    public double OverallAdherenceRate { get; init; }
}

