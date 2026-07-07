using Doto.Application.DTOs;
using Doto.Application.DTOs.Requests.Schedule;
using Doto.Application.DTOs.Responses;

namespace Doto.Application.Interfaces;

public interface IScheduleService
{
    Task<BaseResponse<ScheduleDtoResponse>> AddScheduleAsync(Guid personId, CreateScheduleRequest request);
    Task<BaseResponse<PagedResult<ScheduleDtoResponse>>> GetAllByPersonAsync(Guid personId, int page = 1, int pageSize = 10, DateOnly? date = null);
    Task<BaseResponse<ScheduleDtoResponse?>> GetByIdAsync(Guid personId, Guid id);
    Task<BaseResponse<ScheduleDtoResponse>> UpdateScheduleAsync(Guid personId, UpdateScheduleRequest request);
}