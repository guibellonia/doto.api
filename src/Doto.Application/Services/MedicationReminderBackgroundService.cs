using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Doto.Application.Interfaces;
using Doto.Domain.Entities;
using Doto.Domain.Enums;
using Doto.Domain.Interfaces;

namespace Doto.Application.Services;

public class MedicationReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MedicationReminderBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute

    public MedicationReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MedicationReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Medication Reminder Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in medication reminder background service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Medication Reminder Background Service stopped");
    }

    private async Task CheckAndSendRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var doseRepository = scope.ServiceProvider.GetRequiredService<IMedicineDoseOccurrenceRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTimeOffset.UtcNow;
        var checkWindowEnd = now.AddMinutes(2); // Check for doses scheduled in next 2 minutes

        // Get all pending doses scheduled in the near future (to account for pre-alarm)
        var upcomingDoses = await doseRepository.GetPendingDosesInTimeRangeAsync(now.AddMinutes(-60), checkWindowEnd);

        foreach (var dose in upcomingDoses)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var schedule = dose.Schedule;
                if (schedule == null || !schedule.IsActive)
                    continue;

                var preAlarmMinutes = schedule.PreAlarmMinutes ?? 0;
                var reminderTime = dose.ScheduledAt.AddMinutes(-preAlarmMinutes);

                // Check if reminder time is within current minute window
                // We check if reminderTime is between now and next minute
                if (reminderTime <= now && reminderTime > now.AddMinutes(-1))
                {
                    await SendReminderForDoseAsync(dose, notificationService);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reminder for dose {DoseId}", dose.Id);
            }
        }
    }

    private async Task SendReminderForDoseAsync(
        MedicineDoseOccurrence dose,
        INotificationService notificationService)
    {
        try
        {
            var medicine = dose.Medicine;
            if (medicine == null)
                return;

            var personId = medicine.PersonId;
            var medicineName = medicine.Name;
            var dosageValue = medicine.DosageValue;
            var dosageUnit = medicine.DosageUnit.ToString();

            var dosage = $"{dosageValue} {dosageUnit}";

            await notificationService.SendMedicationReminderAsync(
                personId,
                dose.MedicineId,
                dose.ScheduleId,
                dose.Id,
                medicineName,
                dosage,
                dose.ScheduledAt);

            _logger.LogInformation(
                "Sent reminder for dose {DoseId}, medicine {MedicineName}, scheduled at {ScheduledAt}",
                dose.Id, medicineName, dose.ScheduledAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reminder for dose {DoseId}", dose.Id);
        }
    }
}

