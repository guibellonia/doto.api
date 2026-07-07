using Doto.Application.DTOs;
using Doto.Application.DTOs.Requests.Health;
using Doto.Application.DTOs.Responses;

namespace Doto.Application.Interfaces;

public interface IHealthService
{
    Task<BaseResponse<VitalSignRecordDto>> RegisterBloodPressureAsync(
        Guid personId,
        RegisterBloodPressureRequest request);

    Task<BaseResponse<VitalSignRecordDto>> RegisterBloodSugarAsync(
        Guid personId,
        RegisterBloodSugarRequest request);

    Task<BaseResponse<VitalSignRecordDto>> RegisterWeightAsync(
        Guid personId,
        RegisterWeightRequest request);

    Task<BaseResponse<VitalSignRecordDto>> RegisterHeightAsync(
        Guid personId,
        RegisterHeightRequest request);

    Task<BaseResponse<SymptomRecordDto>> RegisterSymptomAsync(
        Guid personId,
        RegisterSymptomRequest request);

    Task<BaseResponse<IReadOnlyList<VitalSignRecordDto>>> GetVitalSignsAsync(
        Guid personId);

    Task<BaseResponse<VitalSignRecordDto?>> GetLatestVitalSignByTypeAsync(
        Guid personId,
        int type);

    Task<BaseResponse<IReadOnlyList<SymptomRecordDto>>> GetSymptomsAsync(
        Guid personId);
}

