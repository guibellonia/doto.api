using Doto.Domain.Common;
using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class Medication : AuditableEntity, ISoftDeletable
{
    private readonly List<MedicationSchedule> _schedules = [];

    private Medication()
    {
        Name = null!;
    }

    private Medication(
        Guid userId,
        string name,
        decimal dosageValue,
        DosageUnit dosageUnit,
        DateOnly treatmentStartDate,
        DateOnly? treatmentEndDate,
        int? totalDoses,
        string? observations)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("O medicamento precisa pertencer a um usuário.", nameof(userId));
        }

        if (dosageValue <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dosageValue), "A dose deve ser positiva.");
        }

        if (treatmentEndDate is not null && treatmentEndDate < treatmentStartDate)
        {
            throw new ArgumentException(
                "O fim do tratamento não pode ser anterior ao início.",
                nameof(treatmentEndDate));
        }

        if (totalDoses is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalDoses), "O total de doses deve ser positivo.");
        }

        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("O nome do medicamento é obrigatório.", nameof(name))
            : name.Trim();

        UserId = userId;
        DosageValue = dosageValue;
        DosageUnit = dosageUnit;
        TreatmentStartDate = treatmentStartDate;
        TreatmentEndDate = treatmentEndDate;
        TotalDoses = totalDoses;
        Observations = string.IsNullOrWhiteSpace(observations) ? null : observations.Trim();
        IsActive = true;
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public decimal DosageValue { get; private set; }

    public DosageUnit DosageUnit { get; private set; }

    public string? Observations { get; private set; }

    public DateOnly TreatmentStartDate { get; private set; }

    public DateOnly? TreatmentEndDate { get; private set; }

    public int? TotalDoses { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public AppUser? User { get; private set; }

    public IReadOnlyCollection<MedicationSchedule> Schedules => _schedules;

    public static Medication Create(
        Guid userId,
        string name,
        decimal dosageValue,
        DosageUnit dosageUnit,
        DateOnly treatmentStartDate,
        DateOnly? treatmentEndDate = null,
        int? totalDoses = null,
        string? observations = null)
        => new(userId, name, dosageValue, dosageUnit, treatmentStartDate, treatmentEndDate, totalDoses, observations);

    public void UpdateDetails(string name, decimal dosageValue, DosageUnit dosageUnit, string? observations)
    {
        if (dosageValue <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dosageValue), "A dose deve ser positiva.");
        }

        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("O nome do medicamento é obrigatório.", nameof(name))
            : name.Trim();
        DosageValue = dosageValue;
        DosageUnit = dosageUnit;
        Observations = string.IsNullOrWhiteSpace(observations) ? null : observations.Trim();
        Touch();
    }

    public void UpdateTreatmentPeriod(DateOnly startDate, DateOnly? endDate, int? totalDoses)
    {
        if (endDate is not null && endDate < startDate)
        {
            throw new ArgumentException("O fim do tratamento não pode ser anterior ao início.", nameof(endDate));
        }

        if (totalDoses is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalDoses), "O total de doses deve ser positivo.");
        }

        TreatmentStartDate = startDate;
        TreatmentEndDate = endDate;
        TotalDoses = totalDoses;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch();
    }

    public void Reactivate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch();
    }

    public void SoftDelete(DateTime deletedAtUtc)
    {
        if (DeletedAt is not null)
        {
            return;
        }

        DeletedAt = deletedAtUtc;
        IsActive = false;
        Touch();
    }
}
