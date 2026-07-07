namespace Doto.Application.DTOs.Responses;

public record VitalSignRecordItem
{
    public Guid Id { get; init; }
    public int Type { get; init; } // VitalSignType enum
    public float Value { get; init; }
    public string? Unit { get; init; }
    public float? SecondaryValue { get; init; } // For blood pressure (diastolic)
    public DateTime RecordedAt { get; init; }
    public string? Notes { get; init; }
}

public record SymptomRecordItem
{
    public Guid Id { get; init; }
    public string Symptoms { get; init; } = string.Empty;
    public int? Severity { get; init; } // 1-10 scale
    public DateTime RecordedAt { get; init; }
    public string? Notes { get; init; }
}

public record VitalSignsReportData
{
    public double? WeightKg { get; init; }
    public double? HeightCm { get; init; }
    public string? BloodPressure { get; init; } // Última pressão (mantido para compatibilidade)
    public double? BloodSugar { get; init; } // Última glicose (mantido para compatibilidade)
    public DateOnly? LastUpdated { get; init; }
    // Histórico de registros
    public List<VitalSignRecordItem>? BloodPressureRecords { get; init; }
    public List<VitalSignRecordItem>? BloodSugarRecords { get; init; }
    public List<VitalSignRecordItem>? WeightRecords { get; init; }
    public List<VitalSignRecordItem>? HeightRecords { get; init; }
}

public record SymptomsReportData
{
    public List<SymptomRecordItem> Symptoms { get; init; } = new();
    public int TotalSymptoms { get; init; }
}

