namespace Doto.Application.DTOs.Responses;

public record MedicinesReportData
{
    public List<MedicineReportItem> Medicines { get; init; } = new();
    public int TotalMedicines { get; init; }
    public int ActiveMedicines { get; init; }
    public double AdherenceRate { get; init; }
}

