using Doto.Application.DTOs;
using Doto.Application.DTOs.Requests.Medicine;
using Doto.Application.DTOs.Responses;

namespace Doto.Application.Interfaces;

public interface IMedicineService
{
    Task<BaseResponse<MedicineDtoResponse>> AddMedicineAsync(Guid personId, CreateMedicineRequest request);
    Task<BaseResponse<PagedResult<MedicineDtoResponse>>> GetAllByPersonAsync(Guid personId, int page = 1, int pageSize = 10);
    Task<BaseResponse<MedicineDtoResponse>> UpdateMedicineAsync(Guid personId, UpdateMedicineRequest request);
    Task<BaseResponse<MedicineDtoResponse?>> GetByIdAsync(Guid personId, Guid id);
    Task<BaseResponse<bool>> SoftDeleteAsync(Guid personId, Guid id);
}
