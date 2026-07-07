using Doto.Application.DTOs;
using Doto.Application.DTOs.Requests.Medicine;
using Doto.Application.DTOs.Responses;
using Doto.Application.Interfaces;
using Doto.Domain.Entities;
using Doto.Domain.Interfaces;

namespace Doto.Application.Services;

public class MedicineService : IMedicineService
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IMedicineDoseOccurrenceRepository _doseOccurrenceRepository;

    public MedicineService(
        IMedicineRepository medicineRepository,
        IScheduleRepository scheduleRepository,
        IMedicineDoseOccurrenceRepository doseOccurrenceRepository)
    {
        _medicineRepository = medicineRepository;
        _scheduleRepository = scheduleRepository;
        _doseOccurrenceRepository = doseOccurrenceRepository;
    }

    public async Task<BaseResponse<MedicineDtoResponse>> AddMedicineAsync(Guid personId, CreateMedicineRequest request)
    {
        var medicine = new Medicine(
            Guid.NewGuid(),
            personId,
            request.Name,
            request.DosageValue,
            request.DosageUnit,
            request.StartDate,
            request.EndDate,
            request.Observations
        );

        await _medicineRepository.AddAsync(medicine);

        var dto = new MedicineDtoResponse(
            medicine.Id,
            medicine.Name,
            medicine.DosageValue,
            medicine.DosageUnit,
            medicine.StartDate,
            medicine.EndDate,
            medicine.Observations
        );

        return BaseResponse<MedicineDtoResponse>.Ok("Medicine created successfully", dto);
    }

    public async Task<BaseResponse<PagedResult<MedicineDtoResponse>>> GetAllByPersonAsync(Guid personId, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var (medicines, total) = await _medicineRepository.GetByPersonIdPagedAsync(personId, page, pageSize);

        var list = medicines.Select(m => new MedicineDtoResponse(
            m.Id,
            m.Name,
            m.DosageValue,
            m.DosageUnit,
            m.StartDate,
            m.EndDate,
            m.Observations
        )).ToList();

        var paged = new PagedResult<MedicineDtoResponse>(list, total, page, pageSize);
        return BaseResponse<PagedResult<MedicineDtoResponse>>.Ok("Medicines fetched successfully", paged);
    }

    public async Task<BaseResponse<MedicineDtoResponse>> UpdateMedicineAsync(Guid personId, UpdateMedicineRequest request)
    {
        var medicine = new Medicine(
            request.Id,
            personId,
            request.Name,
            request.DosageValue,
            request.DosageUnit,
            request.StartDate,
            request.EndDate,
            request.Observations
        );

        await _medicineRepository.UpdateAsync(medicine);

        var dto = new MedicineDtoResponse(
            medicine.Id,
            medicine.Name,
            medicine.DosageValue,
            medicine.DosageUnit,
            medicine.StartDate,
            medicine.EndDate,
            medicine.Observations
        );

        return BaseResponse<MedicineDtoResponse>.Ok("Medicine updated successfully", dto);
    }

    public async Task<BaseResponse<MedicineDtoResponse?>> GetByIdAsync(Guid personId, Guid id)
    {
        var medicine = await _medicineRepository.GetByIdAsync(id);
        if (medicine == null) return BaseResponse<MedicineDtoResponse?>.Fail("Medicine not found");
        if (medicine.PersonId != personId) return BaseResponse<MedicineDtoResponse?>.Fail("Medicine not found or access denied");

        var dto = new MedicineDtoResponse(
            medicine.Id,
            medicine.Name,
            medicine.DosageValue,
            medicine.DosageUnit,
            medicine.StartDate,
            medicine.EndDate,
            medicine.Observations
        );

        return BaseResponse<MedicineDtoResponse?>.Ok("Medicine fetched successfully", dto);
    }

    public async Task<BaseResponse<bool>> SoftDeleteAsync(Guid personId, Guid id)
    {
        var medicine = await _medicineRepository.GetByIdAsync(id);
        if (medicine == null)
            return BaseResponse<bool>.Fail("Medicine not found", false);
        
        if (medicine.PersonId != personId)
            return BaseResponse<bool>.Fail("Medicine not found or access denied", false);

        // Buscar todos os schedules da medicação (incluindo inativos para verificar)
        var allSchedules = await _scheduleRepository.GetAllByMedicineIdAsync(id);
        
        // Para cada schedule, verificar se tem doses tomadas
        // Se não tiver, desativar o schedule
        foreach (var schedule in allSchedules)
        {
            var hasTakenDoses = await _doseOccurrenceRepository.HasTakenDosesAsync(schedule.Id);
            if (!hasTakenDoses && schedule.IsActive)
            {
                schedule.Deactivate();
                await _scheduleRepository.UpdateAsync(schedule);
            }
        }

        await _medicineRepository.SoftDeleteAsync(id);
        return BaseResponse<bool>.Ok("Medicine deleted successfully", true);
    }
}
