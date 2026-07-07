using Doto.Domain.Entities;

namespace Doto.Application.DTOs.Responses;

public record DoseOccurrenceDto(
    Guid Id,
    Guid MedicineId,
    Guid ScheduleId,
    string MedicineName,
    float DosageValue,
    string DosageUnit,
    DateTimeOffset ScheduledAt,
    string Status,
    DateTimeOffset? TakenAt,
    DateTimeOffset? SnoozedUntil
)
{
    public static DoseOccurrenceDto FromEntity(MedicineDoseOccurrence dose)
    {
        var med = dose.Medicine;

        return new DoseOccurrenceDto(
            Id: dose.Id,
            MedicineId: dose.MedicineId,
            ScheduleId: dose.ScheduleId,
            MedicineName: med.Name,
            DosageValue: med.DosageValue,
            DosageUnit: med.DosageUnit.ToString(),
            ScheduledAt: dose.ScheduledAt,
            Status: dose.Status.ToString(),
            TakenAt: dose.TakenAt,
            SnoozedUntil: dose.SnoozedUntil
        );
    }
}
