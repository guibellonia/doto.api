namespace Doto.Application.Interfaces;

public interface INotificationService
{
    Task SendMedicationReminderAsync(
        Guid personId,
        Guid medicineId,
        Guid scheduleId,
        Guid doseOccurrenceId,
        string medicineName,
        string dosage,
        DateTimeOffset scheduledAt);
    
    Task SendMedicationTakenNotificationAsync(
        Guid personId,
        Guid? sponsorId,
        string medicineName,
        DateTimeOffset takenAt);
}

