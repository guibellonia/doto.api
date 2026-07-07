using Doto.Application.DTOs;
using Doto.Application.DTOs.Requests.Schedule;
using Doto.Application.DTOs.Responses;
using Doto.Application.Helpers;
using Doto.Application.Interfaces;
using Doto.Domain.Entities;
using Doto.Domain.Interfaces;

namespace Doto.Application.Services;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IMedicineRepository _medicineRepository;
    private readonly IMedicineAdherenceService _medicineAdherenceService;
    private readonly IMedicineDoseOccurrenceRepository _doseOccurrenceRepository;

    public ScheduleService(
        IScheduleRepository scheduleRepository, 
        IMedicineRepository medicineRepository, 
        IMedicineAdherenceService medicineAdherenceService,
        IMedicineDoseOccurrenceRepository doseOccurrenceRepository)
    {
        _scheduleRepository = scheduleRepository;
        _medicineRepository = medicineRepository;
        _medicineAdherenceService = medicineAdherenceService;
        _doseOccurrenceRepository = doseOccurrenceRepository;
    }

    public async Task<BaseResponse<ScheduleDtoResponse>> AddScheduleAsync(Guid personId, CreateScheduleRequest request)
    {
        var medicine = await _medicineRepository.GetByIdAsync(request.MedicineId)
            ?? throw new InvalidOperationException("Medicine not found");

        if (medicine.PersonId != personId)
            throw new UnauthorizedAccessException("User does not own the medicine");

        var validationError = ScheduleHelper.ValidateCreateRequest(request);
        if (validationError is not null)
            return BaseResponse<ScheduleDtoResponse>.Fail(validationError);

        var schedule = new Schedule(id: Guid.NewGuid(), medicineId: request.MedicineId, scheduleType: request.ScheduleType);

        ScheduleHelper.ApplyScheduleConfiguration(schedule, request);

        schedule.SetAlarmConfig(request.PreAlarmMinutes, request.PosAlarmMinutes);

        await _scheduleRepository.AddAsync(schedule);
        await _medicineAdherenceService.GenerateFutureDosesForMedicineAsync(request.MedicineId);

        var dto = ScheduleDtoResponse.FromEntity(schedule);

        return BaseResponse<ScheduleDtoResponse>.Ok("Schedule created successfully", dto);
    }

    public async Task<BaseResponse<PagedResult<ScheduleDtoResponse>>> GetAllByPersonAsync(Guid personId, int page = 1, int pageSize = 10, DateOnly? date = null)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var (schedules, total) = await _scheduleRepository.GetByPersonIdPagedAsync(personId, page, pageSize);

        List<ScheduleDtoResponse> list;
        
        // Se uma data for fornecida, incluir as dose occurrences relacionadas
        if (date.HasValue)
        {
            var scheduleIds = schedules.Select(s => s.Id).ToList();
            var doseOccurrences = await _doseOccurrenceRepository.GetByScheduleIdsAndDateAsync(scheduleIds, date.Value);
            
            // Agrupar dose occurrences por scheduleId
            var doseOccurrencesByScheduleId = doseOccurrences
                .GroupBy(d => d.ScheduleId)
                .ToDictionary(g => g.Key, g => g.Select(DoseOccurrenceDto.FromEntity).ToList());

            list = schedules.Select(schedule =>
            {
                var doseOccurrencesForSchedule = doseOccurrencesByScheduleId.GetValueOrDefault(schedule.Id);
                return ScheduleDtoResponse.FromEntity(schedule, doseOccurrencesForSchedule);
            }).ToList();
        }
        else
        {
            list = schedules
                .Select(schedule => ScheduleDtoResponse.FromEntity(schedule))
                .ToList();
        }

        var paged = new PagedResult<ScheduleDtoResponse>(list, total, page, pageSize);
        return BaseResponse<PagedResult<ScheduleDtoResponse>>.Ok("Schedules fetched successfully", paged);
    }

    public async Task<BaseResponse<ScheduleDtoResponse?>> GetByIdAsync(Guid personId, Guid id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
            return BaseResponse<ScheduleDtoResponse?>.Fail("Schedule not found");

        if (schedule.Medicine == null || schedule.Medicine.PersonId != personId)
            return BaseResponse<ScheduleDtoResponse?>.Fail("Schedule not found or access denied");

        var dto = ScheduleDtoResponse.FromEntity(schedule);

        return BaseResponse<ScheduleDtoResponse?>.Ok("Schedule fetched successfully", dto);
    }

    public async Task<BaseResponse<ScheduleDtoResponse>> UpdateScheduleAsync(Guid personId, UpdateScheduleRequest request)
    {
        var existing = await _scheduleRepository.GetByIdAsync(request.Id)
            ?? throw new InvalidOperationException("Schedule not found");

        if (existing.Medicine == null || existing.Medicine.PersonId != personId)
            throw new UnauthorizedAccessException("User does not own the schedule");

        if (request.MedicineId != existing.MedicineId)
        {
            var newMed = await _medicineRepository.GetByIdAsync(request.MedicineId)
                ?? throw new InvalidOperationException("Target medicine not found");

            if (newMed.PersonId != personId)
                throw new UnauthorizedAccessException("User does not own the target medicine");
        }

        var validationError = ScheduleHelper.ValidateUpdateRequest(request);
        if (validationError is not null)
            return BaseResponse<ScheduleDtoResponse>.Fail(validationError);

        existing.ChangeType(request.ScheduleType);
        ScheduleHelper.ApplyScheduleConfiguration(existing, request);

        existing.SetAlarmConfig(request.PreAlarmMinutes, request.PosAlarmMinutes);

        await _scheduleRepository.UpdateAsync(existing);
        await _medicineAdherenceService.GenerateFutureDosesForMedicineAsync(request.MedicineId);

        var dto = ScheduleDtoResponse.FromEntity(existing);

        return BaseResponse<ScheduleDtoResponse>.Ok("Schedule updated successfully", dto);
    }

}