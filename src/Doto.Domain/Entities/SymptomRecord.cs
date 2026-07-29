using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class SymptomRecord : EntityBase, ISoftDeletable
{
    public const short MinSeverity = 0;
    public const short MaxSeverity = 10;

    private SymptomRecord()
    {
        Name = null!;
    }

    private SymptomRecord(
        Guid userId,
        string name,
        short? severity,
        SymptomTrend trend,
        int? durationMinutes,
        DateTime recordedAtUtc,
        DateOnly recordedLocalDate,
        TimeOnly recordedLocalTime,
        string? notes)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("O registro precisa pertencer a um usuário.", nameof(userId));
        }

        if (severity is < MinSeverity or > MaxSeverity)
        {
            throw new ArgumentOutOfRangeException(nameof(severity), $"A severidade deve estar entre {MinSeverity} e {MaxSeverity}.");
        }

        if (durationMinutes is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMinutes), "A duração não pode ser negativa.");
        }

        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("O nome do sintoma é obrigatório.", nameof(name))
            : name.Trim();

        UserId = userId;
        Severity = severity;
        Trend = trend;
        DurationMinutes = durationMinutes;
        RecordedAtUtc = recordedAtUtc;
        RecordedLocalDate = recordedLocalDate;
        RecordedLocalTime = recordedLocalTime;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public short? Severity { get; private set; }

    public SymptomTrend Trend { get; private set; }

    public int? DurationMinutes { get; private set; }

    public DateTime RecordedAtUtc { get; private set; }

    public DateOnly RecordedLocalDate { get; private set; }

    public TimeOnly RecordedLocalTime { get; private set; }

    public string? Notes { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public AppUser? User { get; private set; }

    public static SymptomRecord Record(
        Guid userId,
        string name,
        DateTime recordedAtUtc,
        DateOnly recordedLocalDate,
        TimeOnly recordedLocalTime,
        short? severity = null,
        SymptomTrend trend = SymptomTrend.Unknown,
        int? durationMinutes = null,
        string? notes = null)
        => new(userId, name, severity, trend, durationMinutes, recordedAtUtc, recordedLocalDate, recordedLocalTime, notes);

    public void Update(string name, short? severity, SymptomTrend trend, int? durationMinutes, string? notes)
    {
        if (severity is < MinSeverity or > MaxSeverity)
        {
            throw new ArgumentOutOfRangeException(nameof(severity), $"A severidade deve estar entre {MinSeverity} e {MaxSeverity}.");
        }

        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("O nome do sintoma é obrigatório.", nameof(name))
            : name.Trim();
        Severity = severity;
        Trend = trend;
        DurationMinutes = durationMinutes;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void SoftDelete(DateTime deletedAtUtc) => DeletedAt ??= deletedAtUtc;
}
