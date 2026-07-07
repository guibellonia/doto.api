using Doto.Application.DTOs.Responses;

namespace Doto.Application.Interfaces;

public interface IAdminAuthService
{
    Task<BaseResponse<bool>> DeleteUserAsync(string userId);
}
