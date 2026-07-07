namespace Doto.Domain.Entities;

public class SymptomRecord
{
    public Guid Id { get; private set; }
    public Guid PersonId { get; private set; }
    public Person Person { get; private set; } = null!;
    public string Symptoms { get; private set; }
    public int? Severity { get; private set; } // 1-10 scale
    public DateTime RecordedAt { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected SymptomRecord() { }

    public SymptomRecord(
        Guid personId,
        string symptoms,
        DateTime recordedAt,
        int? severity = null,
        string? notes = null)
    {
        Id = Guid.NewGuid();
        PersonId = personId;
        Symptoms = symptoms;
        Severity = severity;
        RecordedAt = recordedAt;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string? symptoms = null,
        int? severity = null,
        DateTime? recordedAt = null,
        string? notes = null)
    {
        if (!string.IsNullOrWhiteSpace(symptoms))
            Symptoms = symptoms;
        
        if (severity.HasValue)
            Severity = severity;
        
        if (recordedAt.HasValue)
            RecordedAt = recordedAt.Value;
        
        if (notes != null)
            Notes = notes;
        
        UpdatedAt = DateTime.UtcNow;
    }
}

