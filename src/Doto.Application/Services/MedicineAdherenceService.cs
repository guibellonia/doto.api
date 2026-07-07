using Microsoft.Extensions.Logging;
using Doto.Application.DTOs.Responses;
using Doto.Application.Helpers;
using Doto.Application.Interfaces;
using Doto.Domain.Entities;
using Doto.Domain.Enums;
using Doto.Domain.Interfaces;

namespace Doto.Application.Services;

public class MedicineAdherenceService : IMedicineAdherenceService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPersonRepository _personRepository;
    private readonly IMedicineRepository _medicineRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IMedicineDoseOccurrenceRepository _doseRepository;
    private readonly INotificationService? _notificationService;
    private readonly ILogger<MedicineAdherenceService> _logger;

    public MedicineAdherenceService(
        ICurrentUserService currentUser,
        IPersonRepository personRepository,
        IMedicineRepository medicineRepository,
        IScheduleRepository scheduleRepository,
        IMedicineDoseOccurrenceRepository doseRepository,
        ILogger<MedicineAdherenceService> logger,
        INotificationService? notificationService = null)
    {
        _currentUser = currentUser;
        _personRepository = personRepository;
        _medicineRepository = medicineRepository;
        _scheduleRepository = scheduleRepository;
        _doseRepository = doseRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<BaseResponse<bool>> MarkDoseTakenAsync(Guid doseId, DateTimeOffset takenAt, Guid? effectivePersonId = null)
    {
        try
        {
            // Ensure the date is in UTC and within valid range
            var utcTakenAt = takenAt.ToUniversalTime();
            
            // Validate date range for PostgreSQL (4713 BC to 5874897 AD)
            // But DateTimeOffset range is more restrictive (0001-01-01 to 9999-12-31)
            if (utcTakenAt < DateTimeOffset.MinValue || utcTakenAt > DateTimeOffset.MaxValue)
            {
                _logger.LogWarning("Invalid date range for takenAt: {TakenAt}", utcTakenAt);
                return BaseResponse<bool>.Fail($"Date value out of bounds: {utcTakenAt:O}. Valid range is {DateTimeOffset.MinValue:O} to {DateTimeOffset.MaxValue:O}", false);
            }

            var person = await PersonHelper.GetCurrentPersonAsync(_currentUser, _personRepository);
            if (person == null)
                return BaseResponse<bool>.Fail("User not authenticated.", false);

            var dose = await _doseRepository.GetByIdAsync(doseId);
            if (dose is null)
                return BaseResponse<bool>.Fail("Dose not found.", false);

            // Use effectivePersonId if provided (for members), otherwise use current person ID
            var targetPersonId = effectivePersonId ?? person.Id;

            // Validate that the dose belongs to the target person (member or current user)
            if (dose.Medicine?.PersonId != targetPersonId)
                return BaseResponse<bool>.Fail("Dose not found or access denied.", false);

            _logger.LogInformation("Marking dose {DoseId} as taken at {TakenAt} (UTC: {Utc})", doseId, takenAt, utcTakenAt);
            dose.MarkTaken(utcTakenAt);
            await _doseRepository.UpdateAsync(dose);

            _logger.LogInformation("Dose {DoseId} marked as taken at {TakenAt}", doseId, utcTakenAt);

            await AdjustFutureScheduleIfNeededAsync(dose, utcTakenAt);

            // Send notification that medication was taken
            if (_notificationService != null)
            {
                try
                {
                    var currentPerson = await PersonHelper.GetCurrentPersonAsync(_currentUser, _personRepository);
                    Guid? sponsorId = null;
                    if (currentPerson?.Member == true && !string.IsNullOrEmpty(currentPerson.SupabaseUserSponsorId))
                    {
                        // Find sponsor by SupabaseUserId
                        var sponsor = await _personRepository.GetBySupabaseUserIdAsync(currentPerson.SupabaseUserSponsorId);
                        if (sponsor != null)
                        {
                            sponsorId = sponsor.Id;
                        }
                    }

                    if (currentPerson != null && dose.Medicine != null)
                    {
                        await _notificationService.SendMedicationTakenNotificationAsync(
                            currentPerson.Id,
                            sponsorId,
                            dose.Medicine.Name,
                            utcTakenAt);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send medication taken notification for dose {DoseId}", doseId);
                    // Don't fail the operation if notification fails
                }
            }

            return BaseResponse<bool>.Ok("Dose marked as taken.", true);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogError(ex, "Date out of bounds error while marking dose {DoseId} as taken.", doseId);
            return BaseResponse<bool>.Fail($"Date value out of bounds: {ex.Message}", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while marking dose {DoseId} as taken.", doseId);
            return BaseResponse<bool>.Fail($"Failed to mark dose as taken: {ex.Message}", false);
        }
    }

    public async Task<BaseResponse<bool>> MarkDoseSkippedAsync(Guid doseId, string? reason = null, Guid? effectivePersonId = null)
    {
        try
        {
            var person = await PersonHelper.GetCurrentPersonAsync(_currentUser, _personRepository);
            if (person == null)
                return BaseResponse<bool>.Fail("User not authenticated.", false);

            var dose = await _doseRepository.GetByIdAsync(doseId);
            if (dose is null)
                return BaseResponse<bool>.Fail("Dose not found.", false);

            // Use effectivePersonId if provided (for members), otherwise use current person ID
            var targetPersonId = effectivePersonId ?? person.Id;

            // Validate that the dose belongs to the target person (member or current user)
            if (dose.Medicine?.PersonId != targetPersonId)
                return BaseResponse<bool>.Fail("Dose not found or access denied.", false);

            dose.MarkSkipped(reason);
            await _doseRepository.UpdateAsync(dose);

            _logger.LogInformation("Dose {DoseId} marked as skipped. Reason={Reason}", doseId, reason);

            return BaseResponse<bool>.Ok("Dose marked as skipped.", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while marking dose {DoseId} as skipped.", doseId);
            return BaseResponse<bool>.Fail("Failed to mark dose as skipped.", false);
        }
    }

    public async Task<BaseResponse<bool>> SnoozeDoseAsync(Guid doseId, int delayInMinutes, Guid? effectivePersonId = null)
    {
        try
        {
            var person = await PersonHelper.GetCurrentPersonAsync(_currentUser, _personRepository);
            if (person == null)
                return BaseResponse<bool>.Fail("User not authenticated.", false);

            var dose = await _doseRepository.GetByIdAsync(doseId);
            if (dose is null)
                return BaseResponse<bool>.Fail("Dose not found.", false);

            // Use effectivePersonId if provided (for members), otherwise use current person ID
            var targetPersonId = effectivePersonId ?? person.Id;

            // Validate that the dose belongs to the target person (member or current user)
            if (dose.Medicine?.PersonId != targetPersonId)
                return BaseResponse<bool>.Fail("Dose not found or access denied.", false);

            if (dose.Status != DoseStatus.Pending)
                return BaseResponse<bool>.Fail("Only pending doses can be snoozed.", false);

            var newTime = DateTimeOffset.UtcNow.AddMinutes(delayInMinutes);
            dose.SnoozeUntil(newTime);
            await _doseRepository.UpdateAsync(dose);

            _logger.LogInformation(
                "Dose {DoseId} snoozed until {NewTime} (+{Delay} minutes)",
                doseId, newTime, delayInMinutes);

            return BaseResponse<bool>.Ok("Dose snoozed successfully.", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while snoozing dose {DoseId}.", doseId);
            return BaseResponse<bool>.Fail("Failed to snooze dose.", false);
        }
    }

    public async Task<BaseResponse<IReadOnlyList<DoseOccurrenceDto>>> GetDailyHistoryAsync(DateOnly day, Guid? effectivePersonId = null)
    {
        try
        {
            var person = await PersonHelper.GetCurrentPersonAsync(_currentUser, _personRepository);
            if (person == null)
                return BaseResponse<IReadOnlyList<DoseOccurrenceDto>>.Fail("User not authenticated");

            // Use effectivePersonId if provided (for members), otherwise use current person ID
            var targetPersonId = effectivePersonId ?? person.Id;

            var from = new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var to = new DateTimeOffset(day.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

            var doses = await _doseRepository.GetByPersonAndPeriodAsync(targetPersonId, from, to);

            var dtos = doses
                .OrderBy(d => d.ScheduledAt)
                .Select(DoseOccurrenceDto.FromEntity)
                .ToList()
                .AsReadOnly();

            return BaseResponse<IReadOnlyList<DoseOccurrenceDto>>.Ok("Daily history fetched successfully.", dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching daily history for date {Day}.", day);
            return BaseResponse<IReadOnlyList<DoseOccurrenceDto>>.Fail("Failed to fetch daily history.");
        }
    }

    public async Task<BaseResponse<IReadOnlyList<DoseOccurrenceDto>>> GetMonthlyHistoryAsync(int year, int month, Guid? effectivePersonId = null)
    {
        try
        {
            var person = await PersonHelper.GetCurrentPersonAsync(_currentUser, _personRepository);
            if (person == null)
                return BaseResponse<IReadOnlyList<DoseOccurrenceDto>>.Fail("User not authenticated");

            // Use effectivePersonId if provided (for members), otherwise use current person ID
            var targetPersonId = effectivePersonId ?? person.Id;

            var firstDay = new DateOnly(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var from = new DateTimeOffset(firstDay.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var to = new DateTimeOffset(lastDay.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

            var doses = await _doseRepository.GetByPersonAndPeriodAsync(targetPersonId, from, to);

            var dtos = doses
                .OrderBy(d => d.ScheduledAt)
                .Select(DoseOccurrenceDto.FromEntity)
                .ToList()
                .AsReadOnly();

            return BaseResponse<IReadOnlyList<DoseOccurrenceDto>>.Ok("Monthly history fetched successfully.", dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching monthly history for {Year}-{Month}.", year, month);
            return BaseResponse<IReadOnlyList<DoseOccurrenceDto>>.Fail("Failed to fetch monthly history.");
        }
    }

    private async Task AdjustFutureScheduleIfNeededAsync(MedicineDoseOccurrence dose, DateTimeOffset takenAt)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(dose.ScheduleId);
        if (schedule is null)
            return;

        if (schedule.ScheduleType != MedicineScheduleType.EveryXHours ||
            !schedule.IntervalInHours.HasValue)
            return;

        // Para schedules EveryXHours, não ajustamos o FirstDoseAt quando marca como tomada
        // O schedule mantém o horário original, e as próximas doses são calculadas a partir do horário agendado
        // Apenas regeneramos as doses futuras a partir de agora, mantendo o FirstDoseAt original
        _logger.LogInformation(
            "Regenerating future doses for EveryXHours schedule {ScheduleId} after dose taken at {TakenAt}",
            schedule.Id, takenAt);

        await RegenerateFutureDosesForMedicineAsync(dose.MedicineId, takenAt);
    }

    private async Task RegenerateFutureDosesForMedicineAsync(Guid medicineId, DateTimeOffset from)
    {
        var medicine = await _medicineRepository.GetByIdAsync(medicineId);
        if (medicine is null)
            return;

        var schedules = await _scheduleRepository.GetByMedicineIdAsync(medicineId);
        if (!schedules.Any())
            return;

        DateTimeOffset to;

        if (medicine.EndDate.HasValue)
        {
            var end = medicine.EndDate.Value.ToDateTime(TimeOnly.MaxValue);
            to = new DateTimeOffset(end, TimeSpan.Zero);
        }
        else
        {
            to = from.AddDays(30);
        }
        await _doseRepository.DeleteFuturePendingByMedicineAsync(medicineId, from);

        var occurrences = new List<MedicineDoseOccurrence>();

        foreach (var schedule in schedules.Where(s => s.IsActive))
        {
            // Para schedules EveryXHours, sempre começar do FirstDoseAt (o GenerateEveryXHours já trata o caso de estar no passado)
            DateTimeOffset scheduleFrom = from;
            if (schedule.ScheduleType == MedicineScheduleType.EveryXHours && schedule.FirstDoseAt.HasValue)
            {
                var medicineStartDate = new DateTimeOffset(medicine.StartDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
                var firstDose = schedule.FirstDoseAt.Value;
                
                // Garantir que FirstDoseAt não seja antes da data de início da medicação
                if (firstDose < medicineStartDate)
                {
                    var timeOfDay = TimeOnly.FromTimeSpan(firstDose.TimeOfDay);
                    firstDose = new DateTimeOffset(
                        medicine.StartDate.ToDateTime(timeOfDay),
                        firstDose.Offset);
                }
                
                // Para EveryXHours, sempre começar do FirstDoseAt (mesmo que esteja no passado)
                // O GenerateEveryXHours vai avançar até chegar no 'from' se necessário
                scheduleFrom = firstDose;
            }
            
            occurrences.AddRange(
                GenerateOccurrencesForSchedule(medicine, schedule, scheduleFrom, to));
        }

        if (occurrences.Any())
        {
            await _doseRepository.AddRangeAsync(occurrences);
        }
    }

    private IEnumerable<MedicineDoseOccurrence> GenerateOccurrencesForSchedule(
        Medicine medicine,
        Schedule schedule,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        return schedule.ScheduleType switch
        {
            MedicineScheduleType.OncePerDay =>
                GenerateOncePerDay(medicine, schedule, from, to),

            MedicineScheduleType.MultipleFixedTimesPerDay =>
                GenerateMultipleTimesPerDay(medicine, schedule, from, to),

            MedicineScheduleType.EveryXHours =>
                GenerateEveryXHours(medicine, schedule, from, to),

            MedicineScheduleType.SpecificWeekDays =>
                GenerateSpecificWeekDays(medicine, schedule, from, to),

            _ => Enumerable.Empty<MedicineDoseOccurrence>()
        };
    }

    private IEnumerable<MedicineDoseOccurrence> GenerateOncePerDay(
        Medicine medicine,
        Schedule schedule,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        if (schedule.TimeOfDay is null)
            yield break;

        var currentDate = medicine.StartDate.ToDateTime(schedule.TimeOfDay.Value);
        var endDate = medicine.EndDate?.ToDateTime(schedule.TimeOfDay.Value) ?? to.UtcDateTime;

        var cursor = new DateTimeOffset(currentDate, TimeSpan.Zero);

        while (cursor <= to && cursor <= endDate)
        {
            if (cursor >= from)
            {
                yield return new MedicineDoseOccurrence(
                    Guid.NewGuid(),
                    medicine.Id,
                    schedule.Id,
                    cursor);
            }

            cursor = cursor.AddDays(1);
        }
    }

    private IEnumerable<MedicineDoseOccurrence> GenerateMultipleTimesPerDay(
        Medicine medicine,
        Schedule schedule,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        if (schedule.TimesOfDay == null || !schedule.TimesOfDay.Any())
            yield break;

        var currentDate = medicine.StartDate.ToDateTime(TimeOnly.MinValue);
        var endDate = medicine.EndDate?.ToDateTime(TimeOnly.MaxValue) ?? to.UtcDateTime;

        var dayCursor = currentDate;

        while (dayCursor <= endDate && dayCursor <= to.UtcDateTime)
        {
            foreach (var time in schedule.TimesOfDay.Select(t => t.Time).OrderBy(t => t))
            {
                var dt = new DateTimeOffset(dayCursor.Date.Add(time.ToTimeSpan()), TimeSpan.Zero);
                if (dt >= from && dt <= to)
                {
                    yield return new MedicineDoseOccurrence(
                        Guid.NewGuid(),
                        medicine.Id,
                        schedule.Id,
                        dt);
                }
            }

            dayCursor = dayCursor.AddDays(1);
        }
    }

    private IEnumerable<MedicineDoseOccurrence> GenerateEveryXHours(
        Medicine medicine,
        Schedule schedule,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        if (schedule.IntervalInHours is null || schedule.FirstDoseAt is null)
            yield break;

        var interval = TimeSpan.FromHours(schedule.IntervalInHours.Value);
        
        // Começar do FirstDoseAt, mas garantir que não seja antes da data de início da medicação
        var medicineStartDate = new DateTimeOffset(medicine.StartDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var firstDose = schedule.FirstDoseAt.Value;
        
        // Se FirstDoseAt for antes da data de início, ajustar para a data de início com o mesmo horário
        if (firstDose < medicineStartDate)
        {
            var timeOfDay = TimeOnly.FromTimeSpan(firstDose.TimeOfDay);
            firstDose = new DateTimeOffset(
                medicine.StartDate.ToDateTime(timeOfDay),
                firstDose.Offset);
        }
        
        // Determinar o início do dia atual (from) para gerar todas as doses do dia, mesmo as que já passaram
        var startOfToday = new DateTimeOffset(from.Date, TimeSpan.Zero);
        var endOfToday = startOfToday.AddDays(1);
        
        // Se o FirstDoseAt está no dia de hoje ou antes, começar do FirstDoseAt
        // Caso contrário, começar do início do dia de hoje
        var cursor = firstDose < endOfToday ? firstDose : startOfToday;

        // Se o cursor está antes do início do dia de hoje, avançar até chegar no dia de hoje
        while (cursor < startOfToday)
        {
            cursor = cursor.Add(interval);
        }

        var endDate = medicine.EndDate.HasValue
            ? new DateTimeOffset(medicine.EndDate.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero)
            : to;

        // Gerar doses até o fim ou até a data de término da medicação
        // Para o dia de hoje, gerar todas as doses (mesmo as que já passaram)
        // Para dias futuros, gerar apenas a partir de 'from'
        while (cursor <= to && cursor <= endDate)
        {
            // Se a dose está no dia de hoje, sempre gerar (mesmo que já tenha passado)
            // Se está em um dia futuro, só gerar se for >= from
            if (cursor < endOfToday || cursor >= from)
            {
                yield return new MedicineDoseOccurrence(
                    Guid.NewGuid(),
                    medicine.Id,
                    schedule.Id,
                    cursor);
            }

            cursor = cursor.Add(interval);
        }
    }

    private IEnumerable<MedicineDoseOccurrence> GenerateSpecificWeekDays(
        Medicine medicine,
        Schedule schedule,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        if (schedule.TimeOfDay is null || schedule.WeekDays == null || !schedule.WeekDays.Any())
            yield break;

        var allowedDays = schedule.WeekDays.Select(w => w.DayOfWeek).ToHashSet();

        var currentDate = medicine.StartDate.ToDateTime(schedule.TimeOfDay.Value);
        var endDate = medicine.EndDate?.ToDateTime(schedule.TimeOfDay.Value) ?? to.UtcDateTime;

        var cursor = currentDate;

        while (cursor <= endDate && cursor <= to.UtcDateTime)
        {
            var dotNetDayOfWeek = (int)new DateOnly(cursor.Year, cursor.Month, cursor.Day).DayOfWeek;
            var mappedDay = dotNetDayOfWeek == 0 ? 7 : dotNetDayOfWeek;

            if (allowedDays.Contains(mappedDay))
            {
                var dto = new DateTimeOffset(cursor, TimeSpan.Zero);
                if (dto >= from && dto <= to)
                {
                    yield return new MedicineDoseOccurrence(
                        Guid.NewGuid(),
                        medicine.Id,
                        schedule.Id,
                        dto);
                }
            }

            cursor = cursor.AddDays(1);
        }
    }

    public async Task<BaseResponse<bool>> GenerateFutureDosesForMedicineAsync(Guid medicineId)
    {
        try
        {
            var medicine = await _medicineRepository.GetByIdAsync(medicineId);
            if (medicine is null)
                return BaseResponse<bool>.Fail("Medicine not found.", false);

            // Para schedules EveryXHours, começar da data de início da medicação ou agora, o que for mais recente
            var medicineStartDate = new DateTimeOffset(medicine.StartDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var from = DateTimeOffset.UtcNow < medicineStartDate ? medicineStartDate : DateTimeOffset.UtcNow;

            await RegenerateFutureDosesForMedicineAsync(medicineId, from);

            return BaseResponse<bool>.Ok("Future doses generated successfully.", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error generating future doses for MedicineId={MedicineId}", medicineId);

            return BaseResponse<bool>.Fail("Failed to generate future doses.", false);
        }
    }

    public async Task<BaseResponse<DoseOccurrenceDto?>> GetDoseOccurrenceByMedicineScheduleAndDateAsync(
        Guid medicineId,
        Guid scheduleId,
        DateOnly date,
        Guid? effectivePersonId = null)
    {
        try
        {
            var person = await PersonHelper.GetCurrentPersonAsync(_currentUser, _personRepository);
            if (person == null)
                return BaseResponse<DoseOccurrenceDto?>.Fail("User not authenticated");
            
            // Use effectivePersonId if provided (for members), otherwise use current person ID
            var targetPersonId = effectivePersonId ?? person.Id;
            
            var medicine = await _medicineRepository.GetByIdAsync(medicineId);
            if (medicine == null || medicine.PersonId != targetPersonId)
                return BaseResponse<DoseOccurrenceDto?>.Fail("Medicine not found or access denied");

            var scheduledAt = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var dose = await _doseRepository.GetByMedicineScheduleAndDateAsync(medicineId, scheduleId, scheduledAt);

            // If dose doesn't exist, try to create it automatically
            if (dose == null)
            {
                // Allow creating doses for past dates (retroactive marking) and future dates
                // Check if date is within medicine period
                if (date < medicine.StartDate)
                {
                    return BaseResponse<DoseOccurrenceDto?>.Ok("Dose occurrence not found - date before medicine start date", null);
                }

                if (medicine.EndDate.HasValue && date > medicine.EndDate.Value)
                {
                    return BaseResponse<DoseOccurrenceDto?>.Ok("Dose occurrence not found - date after medicine end date", null);
                }

                // Get schedule to generate the dose
                var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
                if (schedule == null || !schedule.IsActive)
                {
                    return BaseResponse<DoseOccurrenceDto?>.Ok("Schedule not found or inactive", null);
                }

                // Generate all doses for this specific date
                var from = scheduledAt;
                var to = new DateTimeOffset(date.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
                
                var generatedDoses = GenerateOccurrencesForSchedule(medicine, schedule, from, to)
                    .Where(d => {
                        var doseDate = d.ScheduledAt.Date;
                        var targetDate = scheduledAt.Date;
                        return doseDate.Year == targetDate.Year && 
                               doseDate.Month == targetDate.Month && 
                               doseDate.Day == targetDate.Day;
                    })
                    .ToList();

                if (generatedDoses.Any())
                {
                    // Add all generated doses for this date
                    await _doseRepository.AddRangeAsync(generatedDoses);
                    
                    _logger.LogInformation(
                        "Auto-created {Count} dose occurrence(s) for MedicineId={MedicineId}, ScheduleId={ScheduleId}, Date={Date}",
                        generatedDoses.Count, medicineId, scheduleId, date);
                    
                    // Use the first generated dose directly (they're all for the same day)
                    dose = generatedDoses.FirstOrDefault();
                    
                    // If still null, try fetching from repository
                    if (dose == null)
                    {
                        dose = await _doseRepository.GetByMedicineScheduleAndDateAsync(medicineId, scheduleId, scheduledAt);
                    }
                }
            }

            if (dose == null)
                return BaseResponse<DoseOccurrenceDto?>.Ok("Dose occurrence not found", null);

            var dto = DoseOccurrenceDto.FromEntity(dose);
            return BaseResponse<DoseOccurrenceDto?>.Ok("Dose occurrence fetched successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching dose occurrence for MedicineId={MedicineId}, ScheduleId={ScheduleId}, Date={Date}",
                medicineId, scheduleId, date);
            return BaseResponse<DoseOccurrenceDto?>.Fail("Failed to fetch dose occurrence");
        }
    }
}
