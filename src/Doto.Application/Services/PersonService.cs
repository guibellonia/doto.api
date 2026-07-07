using Microsoft.Extensions.Logging;
using Doto.Application.DTOs.Requests;
using Doto.Application.DTOs.Responses;
using Doto.Application.Helpers;
using Doto.Application.Interfaces;
using Doto.Domain.Entities;
using Doto.Domain.Interfaces;

namespace Doto.Application.Services;

public class PersonService : IPersonService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPersonRepository _personRepository;
    private readonly IAdminAuthService _adminAuthService;
    private readonly ILogger<PersonService> _logger;

    public PersonService(
        ICurrentUserService currentUser,
        IPersonRepository personRepository,
        IAdminAuthService adminAuthService,
        ILogger<PersonService> logger)
    {
        _currentUser = currentUser;
        _personRepository = personRepository;
        _adminAuthService = adminAuthService;
        _logger = logger;
    }

    public async Task<BaseResponse<PersonResponseDTO>> GetOrCreateByUserAsync(RegisterPessoaDto request)
    {
        var supabaseUserId = _currentUser.SupabaseUserId
            ?? throw new UnauthorizedAccessException("Invalid or expired token.");

        string? emailFromToken = _currentUser.Email;
        string effectiveEmail = request.IsMember
            ? request.Email
            : (!string.IsNullOrWhiteSpace(emailFromToken) ? emailFromToken : request.Email);

        try
        {
            Person? person;
            string message;

            if (request.IsMember)
            {
                // Para membros, sempre criar uma nova Person (não atualizar existente)
                // Membros não têm SupabaseUserId próprio, apenas SupabaseUserSponsorId
                _logger.LogInformation("Creating new Member for owner SupabaseUserId={Id}", supabaseUserId);

                // Gerar um GUID único como SupabaseUserId temporário para membros
                // Isso garante que cada membro seja único e não substitua outros
                string memberSupabaseId = Guid.NewGuid().ToString();

                person = new Person(
                    supabaseUserId: memberSupabaseId,
                    email: effectiveEmail,
                    name: request.Name,
                    username: request.UserName,
                    birthDate: request.BirthDate,
                    phone: request.Phone,
                    weightKg: request.WeightKg,
                    heightCm: request.HeightCm
                );

                person.RegisterMember(supabaseUserId);

                await _personRepository.AddAsync(person);
                message = "Member created successfully";
            }
            else
            {
                // Para usuários normais, usar a lógica de get or create
                string targetSupabaseId = supabaseUserId;
                person = await _personRepository.GetBySupabaseUserIdAsync(targetSupabaseId);

                if (person is null)
                {
                    _logger.LogInformation("Creating Person for SupabaseUserId={Id}", targetSupabaseId);

                    person = new Person(
                        supabaseUserId: targetSupabaseId,
                        email: effectiveEmail,
                        name: request.Name,
                        username: request.UserName,
                        birthDate: request.BirthDate,
                        phone: request.Phone,
                        weightKg: request.WeightKg,
                        heightCm: request.HeightCm
                    );

                    await _personRepository.AddAsync(person);
                    message = "Person created successfully";
                }
                else
                {
                    _logger.LogInformation("Updating Person (Id={PersonId}) for SupabaseUserId={SupabaseId}", person.Id, targetSupabaseId);

                    person.UpdateProfile(
                        email: effectiveEmail,
                        name: request.Name,
                        username: request.UserName,
                        birthDate: request.BirthDate,
                        phone: request.Phone,
                        weightKg: request.WeightKg,
                        heightCm: request.HeightCm
                    );

                    message = "Person updated successfully";
                }
            }

            await _personRepository.SaveChangesAsync();

            var dto = new PersonResponseDTO(
                person.Id, 
                person.Name, 
                person.Email,
                person.Username,
                person.BirthDate,
                person.Phone,
                person.WeightKg,
                person.HeightCm,
                person.CreatedAt,
                person.UpdatedAt,
                person.SupabaseUserId
            );

            return BaseResponse<PersonResponseDTO>.Ok(message, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create/update person. Rolling back Supabase user...");

            var deleteResponse = await _adminAuthService.DeleteUserAsync(supabaseUserId);

            if (deleteResponse.Success)
                _logger.LogWarning("Rollback successful. Deleted Supabase user {Id}", supabaseUserId);
            else
                _logger.LogError("Rollback failed. Could not delete Supabase user {Id}", supabaseUserId);

            return BaseResponse<PersonResponseDTO>.Fail("Failed to create/update person.");
        }
    }

    public async Task<BaseResponse<PersonResponseDTO>> GetCurrentPerson()
    {
        try
        {
            var person = await PersonHelper.GetCurrentPersonAsync(_currentUser, _personRepository);

            if (person == null)
            {
                return BaseResponse<PersonResponseDTO>.Fail("Person not found");
            }

            var dto = new PersonResponseDTO(
                person.Id, 
                person.Name, 
                person.Email,
                person.Username,
                person.BirthDate,
                person.Phone,
                person.WeightKg,
                person.HeightCm,
                person.CreatedAt,
                person.UpdatedAt,
                person.SupabaseUserId
            );

            return BaseResponse<PersonResponseDTO>.Ok("Person fetched successfully", dto);
        }
        catch (InvalidOperationException ex)
        {
            // Person not found - return failure response instead of throwing
            _logger.LogInformation("Person not found for current user: {Message}", ex.Message);
            return BaseResponse<PersonResponseDTO>.Fail("Person not found. Please complete your registration.");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt: {Message}", ex.Message);
            return BaseResponse<PersonResponseDTO>.Fail("Invalid or expired token.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current person");
            return BaseResponse<PersonResponseDTO>.Fail("An error occurred while fetching person data.");
        }
    }

    public async Task<BaseResponse<List<PersonResponseDTO>>> GetMembersByOwnerAsync(string ownerId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                _logger.LogWarning("GetMembersByOwnerAsync called with null or empty ownerId");
                return BaseResponse<List<PersonResponseDTO>>.Fail("Owner ID is required");
            }

            var members = await _personRepository.GetMembersByOwnerIdAsync(ownerId);
            
            if (members == null)
            {
                return BaseResponse<List<PersonResponseDTO>>.Ok("Members fetched successfully", new List<PersonResponseDTO>());
            }

            var list = members
                .Select(p => new PersonResponseDTO(
                    p.Id, 
                    p.Name, 
                    p.Email,
                    p.Username,
                    p.BirthDate,
                    p.Phone,
                    p.WeightKg,
                    p.HeightCm,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.SupabaseUserId
                ))
                .ToList();

            return BaseResponse<List<PersonResponseDTO>>.Ok("Members fetched successfully", list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching members for ownerId={OwnerId}", ownerId);
            return BaseResponse<List<PersonResponseDTO>>.Fail($"Failed to fetch members: {ex.Message}");
        }
    }

    public async Task<BaseResponse<PersonResponseDTO>> GetMemberByIdAsync(string memberId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(memberId) || !Guid.TryParse(memberId, out var memberGuid))
            {
                return BaseResponse<PersonResponseDTO>.Fail("Invalid member ID format");
            }

            var currentPerson = await PersonHelper.GetCurrentPersonAsync(_currentUser, _personRepository);
            if (currentPerson == null)
            {
                return BaseResponse<PersonResponseDTO>.Fail("Current user not found");
            }

            var member = await _personRepository.GetByIdAsync(memberGuid);
            if (member == null)
            {
                return BaseResponse<PersonResponseDTO>.Fail("Member not found");
            }

            // Validar se o membro pertence ao usuário atual
            if (!member.Member || member.SupabaseUserSponsorId != currentPerson.SupabaseUserId)
            {
                return BaseResponse<PersonResponseDTO>.Fail("Member does not belong to current user");
            }

            var dto = new PersonResponseDTO(
                member.Id,
                member.Name,
                member.Email,
                member.Username,
                member.BirthDate,
                member.Phone,
                member.WeightKg,
                member.HeightCm,
                member.CreatedAt,
                member.UpdatedAt,
                member.SupabaseUserId
            );

            return BaseResponse<PersonResponseDTO>.Ok("Member fetched successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching member with Id={MemberId}", memberId);
            return BaseResponse<PersonResponseDTO>.Fail("Failed to fetch member.");
        }
    }

    public async Task<Guid> GetEffectivePersonIdAsync(string? memberId)
    {
        var currentPerson = await PersonHelper.GetCurrentPersonAsync(_currentUser, _personRepository);
        if (currentPerson == null)
        {
            throw new UnauthorizedAccessException("Current user not found");
        }

        // Se não fornecido memberId, usar o current person
        if (string.IsNullOrWhiteSpace(memberId))
        {
            return currentPerson.Id;
        }

        // Converter string para Guid
        if (!Guid.TryParse(memberId, out var memberGuid))
        {
            throw new InvalidOperationException("Invalid member ID format");
        }

        // Validar se o membro pertence ao usuário atual
        var member = await _personRepository.GetByIdAsync(memberGuid);
        if (member == null)
        {
            throw new InvalidOperationException("Member not found");
        }

        if (!member.Member || member.SupabaseUserSponsorId != currentPerson.SupabaseUserId)
        {
            throw new UnauthorizedAccessException("Member does not belong to current user");
        }

        return member.Id;
    }

    public async Task<BaseResponse<PersonResponseDTO>> UpdatePersonAsync(
        Guid personId,
        UpdatePersonRequest request)
    {
        try
        {
            var currentPerson = await PersonHelper.GetCurrentPersonAsync(_currentUser, _personRepository);
            if (currentPerson == null)
            {
                return BaseResponse<PersonResponseDTO>.Fail("Current user not found");
            }

            // Verificar se é para atualizar a própria pessoa ou um membro
            Person? personToUpdate;
            if (personId == currentPerson.Id)
            {
                // Atualizando a própria pessoa
                personToUpdate = currentPerson;
            }
            else
            {
                // Atualizando um membro - verificar se pertence ao usuário atual
                personToUpdate = await _personRepository.GetByIdAsync(personId);
                if (personToUpdate == null)
                {
                    return BaseResponse<PersonResponseDTO>.Fail("Person not found");
                }

                if (!personToUpdate.Member || personToUpdate.SupabaseUserSponsorId != currentPerson.SupabaseUserId)
                {
                    return BaseResponse<PersonResponseDTO>.Fail("Person does not belong to current user");
                }
            }

            // Atualizar apenas os campos fornecidos
            personToUpdate.UpdateProfile(
                email: request.Email,
                name: request.Name,
                phone: request.Phone,
                weightKg: request.WeightKg,
                heightCm: request.HeightCm
            );

            await _personRepository.SaveChangesAsync();

            var dto = new PersonResponseDTO(
                personToUpdate.Id,
                personToUpdate.Name,
                personToUpdate.Email,
                personToUpdate.Username,
                personToUpdate.BirthDate,
                personToUpdate.Phone,
                personToUpdate.WeightKg,
                personToUpdate.HeightCm,
                personToUpdate.CreatedAt,
                personToUpdate.UpdatedAt,
                personToUpdate.SupabaseUserId
            );

            return BaseResponse<PersonResponseDTO>.Ok("Person updated successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating person with Id={PersonId}", personId);
            return BaseResponse<PersonResponseDTO>.Fail("Failed to update person");
        }
    }
}
