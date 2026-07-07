using Doto.Application.DTOs;
using Doto.Application.DTOs.Requests.Report;
using Doto.Application.DTOs.Responses;
using Doto.Application.Interfaces;
using Doto.Domain.Entities;
using Doto.Domain.Enums;
using Doto.Domain.Interfaces;

namespace Doto.Application.Services;

public class ReportService : IReportService
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IVitalSignRecordRepository _vitalSignRecordRepository;
    private readonly ISymptomRecordRepository _symptomRecordRepository;

    public ReportService(
        IMedicineRepository medicineRepository,
        IScheduleRepository scheduleRepository,
        IPersonRepository personRepository,
        IVitalSignRecordRepository vitalSignRecordRepository,
        ISymptomRecordRepository symptomRecordRepository)
    {
        _medicineRepository = medicineRepository;
        _scheduleRepository = scheduleRepository;
        _personRepository = personRepository;
        _vitalSignRecordRepository = vitalSignRecordRepository;
        _symptomRecordRepository = symptomRecordRepository;
    }

    public async Task<BaseResponse<ReportDtoResponse>> GenerateReportAsync(
        Guid personId,
        GenerateReportRequest request)
    {
        // Validar período
        if (request.StartDate > request.EndDate)
        {
            return BaseResponse<ReportDtoResponse>.Fail("StartDate cannot be greater than EndDate");
        }

        // Validar tipos de relatório
        if (request.ReportTypes == null || request.ReportTypes.Count == 0)
        {
            return BaseResponse<ReportDtoResponse>.Fail("At least one report type must be specified");
        }

        // Buscar pessoa pelo ID
        var person = await _personRepository.GetByIdAsync(personId);
        if (person == null)
        {
            return BaseResponse<ReportDtoResponse>.Fail("Person not found");
        }

        // Buscar todas as medicações da pessoa
        var allMedicines = await _medicineRepository.GetByPersonIdAsync(personId);

        // Filtrar medicações pelo período
        var medicinesInPeriod = allMedicines.Where(m =>
        {
            // Medicação está no período se:
            // - StartDate da medicação <= EndDate do período E
            // - (EndDate da medicação >= StartDate do período OU EndDate da medicação é null)
            var medicineEndsInPeriod = m.EndDate == null || m.EndDate >= request.StartDate;
            var medicineStartsInPeriod = m.StartDate <= request.EndDate;
            return medicineStartsInPeriod && medicineEndsInPeriod;
        }).ToList();

        // Buscar todos os schedules da pessoa
        var allSchedules = await _scheduleRepository.GetByPersonIdAsync(personId);

        // Determinar qual tipo de relatório gerar
        var reportType = request.ReportTypes.Contains(ReportType.Complete)
            ? ReportType.Complete
            : request.ReportTypes.First();

        MedicinesReportData? medicinesData = null;
        VitalSignsReportData? vitalSignsData = null;
        SymptomsReportData? symptomsData = null;
        CompleteReportData? completeData = null;

        // Gerar dados de medicações se necessário
        if (reportType == ReportType.Medicines || reportType == ReportType.Complete)
        {
            medicinesData = GenerateMedicinesReportData(medicinesInPeriod, allSchedules);
        }

        // Gerar dados de informações vitais se necessário
        if (reportType == ReportType.VitalSigns || reportType == ReportType.Complete)
        {
            vitalSignsData = await GenerateVitalSignsReportDataAsync(person, request.StartDate, request.EndDate);
        }

        // Gerar dados de sintomas se necessário
        if (reportType == ReportType.Complete)
        {
            symptomsData = await GenerateSymptomsReportDataAsync(personId, request.StartDate, request.EndDate);
        }

        // Gerar relatório completo se necessário
        if (reportType == ReportType.Complete && medicinesData != null && vitalSignsData != null)
        {
            var overallAdherenceRate = medicinesData.AdherenceRate;
            completeData = new CompleteReportData
            {
                Medicines = medicinesData,
                VitalSigns = vitalSignsData,
                Symptoms = symptomsData,
                OverallAdherenceRate = overallAdherenceRate
            };
        }

        var reportResponse = new ReportDtoResponse
        {
            Type = reportType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MedicinesData = medicinesData,
            VitalSignsData = vitalSignsData,
            SymptomsData = symptomsData,
            CompleteData = completeData
        };

        return BaseResponse<ReportDtoResponse>.Ok("Report generated successfully", reportResponse);
    }

    private MedicinesReportData GenerateMedicinesReportData(
        List<Medicine> medicines,
        List<Schedule> allSchedules)
    {
        var medicineItems = new List<MedicineReportItem>();
        var activeMedicines = 0;

        foreach (var medicine in medicines)
        {
            var medicineSchedules = allSchedules
                .Where(s => s.MedicineId == medicine.Id)
                .ToList();

            var scheduleItems = medicineSchedules.Select(s => new ScheduleReportItem
            {
                ScheduledTime = s.TimeOfDay,
                ScheduleType = s.ScheduleType,
                WeekDays = s.WeekDays.Select(wd => (WeekDay)wd.DayOfWeek).ToList()
            }).ToList();

            var isActive = medicine.EndDate == null || medicine.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow);

            if (isActive)
            {
                activeMedicines++;
            }

            medicineItems.Add(new MedicineReportItem
            {
                Id = medicine.Id,
                Name = medicine.Name,
                DosageValue = medicine.DosageValue,
                DosageUnit = medicine.DosageUnit,
                StartDate = medicine.StartDate,
                EndDate = medicine.EndDate,
                Observations = medicine.Observations,
                Schedules = scheduleItems
            });
        }

        // Calcular taxa de adesão: (medicamentos com schedules / total de medicamentos) * 100
        var medicinesWithSchedules = medicines.Count(m => 
            allSchedules.Any(s => s.MedicineId == m.Id));
        var adherenceRate = medicines.Count > 0
            ? (double)medicinesWithSchedules / medicines.Count * 100
            : 0;

        return new MedicinesReportData
        {
            Medicines = medicineItems,
            TotalMedicines = medicines.Count,
            ActiveMedicines = activeMedicines,
            AdherenceRate = Math.Round(adherenceRate, 2)
        };
    }

    private async Task<VitalSignsReportData> GenerateVitalSignsReportDataAsync(
        Person person,
        DateOnly startDate,
        DateOnly endDate)
    {
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

        // Buscar todos os registros vitais da pessoa no período
        var allVitalSigns = await _vitalSignRecordRepository.GetByPersonIdAsync(person.Id);
        
        // Filtrar por período
        var vitalSignsInPeriod = allVitalSigns
            .Where(vs => vs.RecordedAt >= startDateTime && vs.RecordedAt <= endDateTime)
            .ToList();

        // Separar por tipo
        var bloodPressureRecords = vitalSignsInPeriod
            .Where(vs => vs.Type == VitalSignType.BloodPressure)
            .OrderByDescending(vs => vs.RecordedAt)
            .Select(vs => new VitalSignRecordItem
            {
                Id = vs.Id,
                Type = (int)vs.Type,
                Value = vs.Value,
                Unit = vs.Unit,
                SecondaryValue = vs.SecondaryValue,
                RecordedAt = vs.RecordedAt,
                Notes = vs.Notes
            })
            .ToList();

        var bloodSugarRecords = vitalSignsInPeriod
            .Where(vs => vs.Type == VitalSignType.BloodSugar)
            .OrderByDescending(vs => vs.RecordedAt)
            .Select(vs => new VitalSignRecordItem
            {
                Id = vs.Id,
                Type = (int)vs.Type,
                Value = vs.Value,
                Unit = vs.Unit,
                SecondaryValue = vs.SecondaryValue,
                RecordedAt = vs.RecordedAt,
                Notes = vs.Notes
            })
            .ToList();

        var weightRecords = vitalSignsInPeriod
            .Where(vs => vs.Type == VitalSignType.Weight)
            .OrderByDescending(vs => vs.RecordedAt)
            .Select(vs => new VitalSignRecordItem
            {
                Id = vs.Id,
                Type = (int)vs.Type,
                Value = vs.Value,
                Unit = vs.Unit,
                SecondaryValue = vs.SecondaryValue,
                RecordedAt = vs.RecordedAt,
                Notes = vs.Notes
            })
            .ToList();

        var heightRecords = vitalSignsInPeriod
            .Where(vs => vs.Type == VitalSignType.Height)
            .OrderByDescending(vs => vs.RecordedAt)
            .Select(vs => new VitalSignRecordItem
            {
                Id = vs.Id,
                Type = (int)vs.Type,
                Value = vs.Value,
                Unit = vs.Unit,
                SecondaryValue = vs.SecondaryValue,
                RecordedAt = vs.RecordedAt,
                Notes = vs.Notes
            })
            .ToList();

        // Buscar últimos valores para compatibilidade
        var lastBloodPressure = allVitalSigns
            .Where(vs => vs.Type == VitalSignType.BloodPressure)
            .OrderByDescending(vs => vs.RecordedAt)
            .FirstOrDefault();

        var lastBloodSugar = allVitalSigns
            .Where(vs => vs.Type == VitalSignType.BloodSugar)
            .OrderByDescending(vs => vs.RecordedAt)
            .FirstOrDefault();

        return new VitalSignsReportData
        {
            WeightKg = person.WeightKg,
            HeightCm = person.HeightCm,
            BloodPressure = lastBloodPressure != null && lastBloodPressure.SecondaryValue.HasValue
                ? $"{lastBloodPressure.Value}/{lastBloodPressure.SecondaryValue.Value} {lastBloodPressure.Unit ?? "mmHg"}"
                : lastBloodPressure != null
                    ? $"{lastBloodPressure.Value} {lastBloodPressure.Unit ?? "mmHg"}"
                    : null,
            BloodSugar = lastBloodSugar != null
                ? lastBloodSugar.Value
                : null,
            LastUpdated = person.UpdatedAt != default 
                ? DateOnly.FromDateTime(person.UpdatedAt) 
                : DateOnly.FromDateTime(person.CreatedAt),
            BloodPressureRecords = bloodPressureRecords.Count > 0 ? bloodPressureRecords : null,
            BloodSugarRecords = bloodSugarRecords.Count > 0 ? bloodSugarRecords : null,
            WeightRecords = weightRecords.Count > 0 ? weightRecords : null,
            HeightRecords = heightRecords.Count > 0 ? heightRecords : null
        };
    }

    private async Task<SymptomsReportData> GenerateSymptomsReportDataAsync(
        Guid personId,
        DateOnly startDate,
        DateOnly endDate)
    {
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

        // Buscar todos os registros de sintomas da pessoa no período
        var allSymptoms = await _symptomRecordRepository.GetByPersonIdAsync(personId);
        
        var symptomsInPeriod = allSymptoms
            .Where(s => s.RecordedAt >= startDateTime && s.RecordedAt <= endDateTime)
            .OrderByDescending(s => s.RecordedAt)
            .Select(s => new SymptomRecordItem
            {
                Id = s.Id,
                Symptoms = s.Symptoms,
                Severity = s.Severity,
                RecordedAt = s.RecordedAt,
                Notes = s.Notes
            })
            .ToList();

        return new SymptomsReportData
        {
            Symptoms = symptomsInPeriod,
            TotalSymptoms = symptomsInPeriod.Count
        };
    }
}

