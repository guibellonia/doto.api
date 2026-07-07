using Doto.Application.DTOs.Responses;
using Doto.Domain.Entities;

namespace Doto.Application.Interfaces
{
    public interface IMedicineAdherenceService
    {
        Task<BaseResponse<bool>> MarkDoseTakenAsync(Guid doseId, DateTimeOffset takenAt, Guid? effectivePersonId = null);
        Task<BaseResponse<bool>> MarkDoseSkippedAsync(Guid doseId, string? reason = null, Guid? effectivePersonId = null);
        Task<BaseResponse<bool>> SnoozeDoseAsync(Guid doseId, int delayInMinutes, Guid? effectivePersonId = null);

        Task<BaseResponse<IReadOnlyList<DoseOccurrenceDto>>> GetDailyHistoryAsync(DateOnly day, Guid? effectivePersonId = null);
        Task<BaseResponse<IReadOnlyList<DoseOccurrenceDto>>> GetMonthlyHistoryAsync(int year, int month, Guid? effectivePersonId = null);
        Task<BaseResponse<bool>> GenerateFutureDosesForMedicineAsync(Guid medicineId);
        Task<BaseResponse<DoseOccurrenceDto?>> GetDoseOccurrenceByMedicineScheduleAndDateAsync(Guid medicineId, Guid scheduleId, DateOnly date, Guid? effectivePersonId = null);
    }
}
