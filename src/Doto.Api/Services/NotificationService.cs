using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Doto.Api.Hubs;
using Doto.Application.Interfaces;
using Doto.Domain.Interfaces;

namespace Doto.Api.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        IPersonRepository personRepository,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task SendMedicationReminderAsync(
        Guid personId,
        Guid medicineId,
        Guid scheduleId,
        Guid doseOccurrenceId,
        string medicineName,
        string dosage,
        DateTimeOffset scheduledAt)
    {
        try
        {
            var message = $"Hora de tomar {medicineName} - {dosage}";
            
            // Send to user
            await _hubContext.Clients.Group($"user_{personId}")
                .SendAsync("MedicationReminder", new
                {
                    DoseOccurrenceId = doseOccurrenceId,
                    MedicineId = medicineId,
                    ScheduleId = scheduleId,
                    MedicineName = medicineName,
                    Dosage = dosage,
                    ScheduledAt = scheduledAt,
                    Message = message
                });

            // Also send to sponsor if user is a member
            var person = await _personRepository.GetByIdAsync(personId);
            if (person != null && person.Member && !string.IsNullOrEmpty(person.SupabaseUserSponsorId))
            {
                var sponsor = await _personRepository.GetBySupabaseUserIdAsync(person.SupabaseUserSponsorId);
                if (sponsor != null)
                {
                    await _hubContext.Clients.Group($"user_{sponsor.Id}")
                        .SendAsync("MedicationReminder", new
                        {
                            DoseOccurrenceId = doseOccurrenceId,
                            MedicineId = medicineId,
                            ScheduleId = scheduleId,
                            MedicineName = medicineName,
                            Dosage = dosage,
                            ScheduledAt = scheduledAt,
                            Message = message,
                            ForMemberId = personId,
                            ForMemberName = person.Name
                        });
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - notifications are non-critical
            _logger.LogWarning(ex, "Error sending medication reminder for person {PersonId}, medicine {MedicineName}", personId, medicineName);
        }
    }

    public async Task SendMedicationTakenNotificationAsync(
        Guid personId,
        Guid? sponsorId,
        string medicineName,
        DateTimeOffset takenAt)
    {
        try
        {
            var message = $"{medicineName} foi tomada";

            // Send to user
            await _hubContext.Clients.Group($"user_{personId}")
                .SendAsync("MedicationTaken", new
                {
                    MedicineName = medicineName,
                    TakenAt = takenAt,
                    Message = message
                });

            // Send to sponsor if provided
            if (sponsorId.HasValue)
            {
                await _hubContext.Clients.Group($"user_{sponsorId.Value}")
                    .SendAsync("MedicationTaken", new
                    {
                        MedicineName = medicineName,
                        TakenAt = takenAt,
                        Message = message,
                        ForMemberId = personId
                    });
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw
            _logger.LogWarning(ex, "Error sending medication taken notification for person {PersonId}, medicine {MedicineName}", personId, medicineName);
        }
    }
}

