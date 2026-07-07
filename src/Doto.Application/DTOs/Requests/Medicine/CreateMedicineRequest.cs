using Doto.Domain.Enums;

namespace Doto.Application.DTOs.Requests.Medicine;

public record CreateMedicineRequest(
    string Name,
    float DosageValue,
    DosageUnit DosageUnit,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Observations
);
