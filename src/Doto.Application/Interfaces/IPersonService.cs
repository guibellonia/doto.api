using Doto.Application.DTOs.Requests;
using Doto.Application.DTOs.Responses;
using Doto.Domain.Entities;


namespace Doto.Application.Interfaces;

public interface IPersonService
{
    Task<BaseResponse<PersonResponseDTO>> GetOrCreateByUserAsync(RegisterPessoaDto request);
    Task<BaseResponse<PersonResponseDTO>> GetCurrentPerson();
    Task<BaseResponse<List<PersonResponseDTO>>> GetMembersByOwnerAsync(string ownerId);
    Task<BaseResponse<PersonResponseDTO>> GetMemberByIdAsync(string memberId);
    Task<Guid> GetEffectivePersonIdAsync(string? memberId);
    Task<BaseResponse<PersonResponseDTO>> UpdatePersonAsync(Guid personId, UpdatePersonRequest request);
}
