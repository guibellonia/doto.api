using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class VitalReading : EntityBase, ISoftDeletable
{
    private VitalReading()
    {
        Unit = null!;
    }

    private VitalReading(
        Guid userId,
        VitalType type,
        decimal valuePrimary,
        decimal? valueSecondary,
        string unit,
        VitalContext context,
        DateTime measuredAtUtc,
        DateOnly measuredLocalDate,
        TimeOnly measuredLocalTime,
        string? notes)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A medição precisa pertencer a um usuário.", nameof(userId));
        }

        if (valuePrimary <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valuePrimary), "O valor medido deve ser positivo.");
        }

        if (type == VitalType.BloodPressure && valueSecondary is null)
        {
            throw new ArgumentException("A pressão arterial exige a diastólica.", nameof(valueSecondary));
        }

        if (type != VitalType.BloodPressure && valueSecondary is not null)
        {
            throw new ArgumentException($"O tipo {type} não aceita um segundo valor.", nameof(valueSecondary));
        }

        Unit = string.IsNullOrWhiteSpace(unit)
            ? throw new ArgumentException("A unidade é obrigatória.", nameof(unit))
            : unit.Trim();

        UserId = userId;
        Type = type;
        ValuePrimary = valuePrimary;
        ValueSecondary = valueSecondary;
        Context = context;
        MeasuredAtUtc = measuredAtUtc;
        MeasuredLocalDate = measuredLocalDate;
        MeasuredLocalTime = measuredLocalTime;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public Guid UserId { get; private set; }

    public VitalType Type { get; private set; }

    public decimal ValuePrimary { get; private set; }

    public decimal? ValueSecondary { get; private set; }

    public string Unit { get; private set; }

    public VitalContext Context { get; private set; }

    public DateTime MeasuredAtUtc { get; private set; }

    public DateOnly MeasuredLocalDate { get; private set; }

    public TimeOnly MeasuredLocalTime { get; private set; }

    public string? Notes { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public AppUser? User { get; private set; }

    public static VitalReading Record(
        Guid userId,
        VitalType type,
        decimal value,
        string unit,
        DateTime measuredAtUtc,
        DateOnly measuredLocalDate,
        TimeOnly measuredLocalTime,
        VitalContext context = VitalContext.NotApplicable,
        string? notes = null)
        => new(userId, type, value, null, unit, context, measuredAtUtc, measuredLocalDate, measuredLocalTime, notes);

    public static VitalReading RecordBloodPressure(
        Guid userId,
        decimal systolic,
        decimal diastolic,
        DateTime measuredAtUtc,
        DateOnly measuredLocalDate,
        TimeOnly measuredLocalTime,
        VitalContext context = VitalContext.NotApplicable,
        string? notes = null)
        => new(
            userId,
            VitalType.BloodPressure,
            systolic,
            diastolic,
            "mmHg",
            context,
            measuredAtUtc,
            measuredLocalDate,
            measuredLocalTime,
            notes);

    public void UpdateNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    public void SoftDelete(DateTime deletedAtUtc) => DeletedAt ??= deletedAtUtc;
}
