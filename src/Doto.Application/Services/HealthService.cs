using Microsoft.Extensions.Logging;
using Doto.Application.DTOs;
using Doto.Application.DTOs.Requests.Health;
using Doto.Application.DTOs.Responses;
using Doto.Application.Interfaces;
using Doto.Domain.Entities;
using Doto.Domain.Enums;
using Doto.Domain.Interfaces;

namespace Doto.Application.Services;

public class HealthService : IHealthService
{
    private readonly IVitalSignRecordRepository _vitalSignRepository;
    private readonly ISymptomRecordRepository _symptomRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<HealthService> _logger;

    public HealthService(
        IVitalSignRecordRepository vitalSignRepository,
        ISymptomRecordRepository symptomRepository,
        IPersonRepository personRepository,
        ILogger<HealthService> logger)
    {
        _vitalSignRepository = vitalSignRepository;
        _symptomRepository = symptomRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<BaseResponse<VitalSignRecordDto>> RegisterBloodPressureAsync(
        Guid personId,
        RegisterBloodPressureRequest request)
    {
        try
        {
            var person = await _personRepository.GetByIdAsync(personId);
            if (person == null)
            {
                return BaseResponse<VitalSignRecordDto>.Fail("Person not found");
            }

            var record = new VitalSignRecord(
                personId,
                VitalSignType.BloodPressure,
                request.SystolicValue,
                request.RecordedAt,
                unit: "mmHg",
                secondaryValue: request.DiastolicValue,
                notes: request.Notes);

            await _vitalSignRepository.AddAsync(record);

            var dto = new VitalSignRecordDto(
                record.Id,
                (int)record.Type,
                record.Value,
                record.Unit,
                record.SecondaryValue,
                record.RecordedAt,
                record.Notes);

            return BaseResponse<VitalSignRecordDto>.Ok("Blood pressure registered successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering blood pressure for PersonId={PersonId}", personId);
            return BaseResponse<VitalSignRecordDto>.Fail("Failed to register blood pressure");
        }
    }

    public async Task<BaseResponse<VitalSignRecordDto>> RegisterBloodSugarAsync(
        Guid personId,
        RegisterBloodSugarRequest request)
    {
        try
        {
            var person = await _personRepository.GetByIdAsync(personId);
            if (person == null)
            {
                return BaseResponse<VitalSignRecordDto>.Fail("Person not found");
            }

            var record = new VitalSignRecord(
                personId,
                VitalSignType.BloodSugar,
                request.Value,
                request.RecordedAt,
                unit: request.Unit ?? "mg/dL",
                notes: request.Notes);

            await _vitalSignRepository.AddAsync(record);

            var dto = new VitalSignRecordDto(
                record.Id,
                (int)record.Type,
                record.Value,
                record.Unit,
                record.SecondaryValue,
                record.RecordedAt,
                record.Notes);

            return BaseResponse<VitalSignRecordDto>.Ok("Blood sugar registered successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering blood sugar for PersonId={PersonId}", personId);
            return BaseResponse<VitalSignRecordDto>.Fail("Failed to register blood sugar");
        }
    }

    public async Task<BaseResponse<VitalSignRecordDto>> RegisterWeightAsync(
        Guid personId,
        RegisterWeightRequest request)
    {
        try
        {
            var person = await _personRepository.GetByIdAsync(personId);
            if (person == null)
            {
                return BaseResponse<VitalSignRecordDto>.Fail("Person not found");
            }

            // Update person's current weight
            person.UpdateProfile(weightKg: request.WeightKg);
            await _personRepository.SaveChangesAsync();

            var record = new VitalSignRecord(
                personId,
                VitalSignType.Weight,
                request.WeightKg,
                request.RecordedAt,
                unit: "kg",
                notes: request.Notes);

            await _vitalSignRepository.AddAsync(record);

            var dto = new VitalSignRecordDto(
                record.Id,
                (int)record.Type,
                record.Value,
                record.Unit,
                record.SecondaryValue,
                record.RecordedAt,
                record.Notes);

            return BaseResponse<VitalSignRecordDto>.Ok("Weight registered successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering weight for PersonId={PersonId}", personId);
            return BaseResponse<VitalSignRecordDto>.Fail("Failed to register weight");
        }
    }

    public async Task<BaseResponse<VitalSignRecordDto>> RegisterHeightAsync(
        Guid personId,
        RegisterHeightRequest request)
    {
        try
        {
            var person = await _personRepository.GetByIdAsync(personId);
            if (person == null)
            {
                return BaseResponse<VitalSignRecordDto>.Fail("Person not found");
            }

            // Update person's current height
            person.UpdateProfile(heightCm: request.HeightCm);
            await _personRepository.SaveChangesAsync();

            var record = new VitalSignRecord(
                personId,
                VitalSignType.Height,
                request.HeightCm,
                request.RecordedAt,
                unit: "cm",
                notes: request.Notes);

            await _vitalSignRepository.AddAsync(record);

            var dto = new VitalSignRecordDto(
                record.Id,
                (int)record.Type,
                record.Value,
                record.Unit,
                record.SecondaryValue,
                record.RecordedAt,
                record.Notes);

            return BaseResponse<VitalSignRecordDto>.Ok("Height registered successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering height for PersonId={PersonId}", personId);
            return BaseResponse<VitalSignRecordDto>.Fail("Failed to register height");
        }
    }

    public async Task<BaseResponse<SymptomRecordDto>> RegisterSymptomAsync(
        Guid personId,
        RegisterSymptomRequest request)
    {
        try
        {
            var person = await _personRepository.GetByIdAsync(personId);
            if (person == null)
            {
                return BaseResponse<SymptomRecordDto>.Fail("Person not found");
            }

            var record = new SymptomRecord(
                personId,
                request.Symptoms,
                request.RecordedAt,
                request.Severity,
                request.Notes);

            await _symptomRepository.AddAsync(record);

            var dto = new SymptomRecordDto(
                record.Id,
                record.Symptoms,
                record.Severity,
                record.RecordedAt,
                record.Notes);

            return BaseResponse<SymptomRecordDto>.Ok("Symptom registered successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering symptom for PersonId={PersonId}", personId);
            return BaseResponse<SymptomRecordDto>.Fail("Failed to register symptom");
        }
    }

    public async Task<BaseResponse<IReadOnlyList<VitalSignRecordDto>>> GetVitalSignsAsync(
        Guid personId)
    {
        try
        {
            var records = await _vitalSignRepository.GetByPersonIdAsync(personId);

            var dtos = records.Select(r => new VitalSignRecordDto(
                r.Id,
                (int)r.Type,
                r.Value,
                r.Unit,
                r.SecondaryValue,
                r.RecordedAt,
                r.Notes)).ToList();

            return BaseResponse<IReadOnlyList<VitalSignRecordDto>>.Ok(
                "Vital signs fetched successfully",
                dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching vital signs for PersonId={PersonId}", personId);
            return BaseResponse<IReadOnlyList<VitalSignRecordDto>>.Fail("Failed to fetch vital signs");
        }
    }

    public async Task<BaseResponse<VitalSignRecordDto?>> GetLatestVitalSignByTypeAsync(
        Guid personId,
        int type)
    {
        try
        {
            var record = await _vitalSignRepository.GetLatestByPersonIdAndTypeAsync(personId, type);

            if (record == null)
            {
                return BaseResponse<VitalSignRecordDto?>.Ok(
                    "No vital sign record found",
                    null);
            }

            var dto = new VitalSignRecordDto(
                record.Id,
                (int)record.Type,
                record.Value,
                record.Unit,
                record.SecondaryValue,
                record.RecordedAt,
                record.Notes);

            return BaseResponse<VitalSignRecordDto?>.Ok(
                "Latest vital sign fetched successfully",
                dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest vital sign for PersonId={PersonId}, Type={Type}", personId, type);
            return BaseResponse<VitalSignRecordDto?>.Fail("Failed to fetch latest vital sign");
        }
    }

    public async Task<BaseResponse<IReadOnlyList<SymptomRecordDto>>> GetSymptomsAsync(
        Guid personId)
    {
        try
        {
            var records = await _symptomRepository.GetByPersonIdAsync(personId);

            var dtos = records.Select(r => new SymptomRecordDto(
                r.Id,
                r.Symptoms,
                r.Severity,
                r.RecordedAt,
                r.Notes)).ToList();

            return BaseResponse<IReadOnlyList<SymptomRecordDto>>.Ok(
                "Symptoms fetched successfully",
                dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching symptoms for PersonId={PersonId}", personId);
            return BaseResponse<IReadOnlyList<SymptomRecordDto>>.Fail("Failed to fetch symptoms");
        }
    }
}

