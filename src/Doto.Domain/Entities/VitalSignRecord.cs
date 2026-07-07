using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class VitalSignRecord
{
    public Guid Id { get; private set; }
    public Guid PersonId { get; private set; }
    public Person Person { get; private set; } = null!;
    public VitalSignType Type { get; private set; }
    public float Value { get; private set; }
    public string? Unit { get; private set; }
    public float? SecondaryValue { get; private set; } // For blood pressure (systolic/diastolic)
    public DateTime RecordedAt { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected VitalSignRecord() { }

    public VitalSignRecord(
        Guid personId,
        VitalSignType type,
        float value,
        DateTime recordedAt,
        string? unit = null,
        float? secondaryValue = null,
        string? notes = null)
    {
        Id = Guid.NewGuid();
        PersonId = personId;
        Type = type;
        Value = value;
        SecondaryValue = secondaryValue;
        Unit = unit;
        RecordedAt = recordedAt;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        float? value = null,
        float? secondaryValue = null,
        string? unit = null,
        DateTime? recordedAt = null,
        string? notes = null)
    {
        if (value.HasValue)
            Value = value.Value;
        
        if (secondaryValue.HasValue)
            SecondaryValue = secondaryValue;
        
        if (unit != null)
            Unit = unit;
        
        if (recordedAt.HasValue)
            RecordedAt = recordedAt.Value;
        
        if (notes != null)
            Notes = notes;
        
        UpdatedAt = DateTime.UtcNow;
    }
}

