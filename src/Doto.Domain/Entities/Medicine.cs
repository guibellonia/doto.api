using Doto.Domain.Enums;

namespace Doto.Domain.Entities;

public class Medicine
{
    public Guid Id { get; private set; }
    public Guid PersonId { get; private set; }
    public string Name { get; private set; }
    public float DosageValue { get; private set; }
    public DosageUnit DosageUnit { get; private set; } 
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; } 
    public string? Observations { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Person? Person { get; private set; }
    public ICollection<MedicineDoseOccurrence> DoseOccurrences { get; private set; } = new List<MedicineDoseOccurrence>();

    protected Medicine() { }

    public Medicine(
        Guid id,
        Guid personId,
        string name,
        float dosageValue,
        DosageUnit dosageUnit,
        DateOnly startDate,
        DateOnly? endDate = null,
        string? observations = null)
    {
        Id = id;
        PersonId = personId;
        Name = name;
        DosageValue = dosageValue;
        DosageUnit = dosageUnit;
        StartDate = startDate;
        EndDate = endDate;
        Observations = observations;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
