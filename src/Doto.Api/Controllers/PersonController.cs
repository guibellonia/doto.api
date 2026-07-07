using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Doto.Application.DTOs.Requests;
using Doto.Application.DTOs.Responses;
using Doto.Application.Interfaces;
using Supabase.Functions.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doto.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private readonly IPersonService _personService;

    public PersonController(IPersonService personService)
    {
        _personService = personService;
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse>> Register([FromBody] RegisterPessoaDto request)
    {
        var result = await _personService.GetOrCreateByUserAsync(request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<PersonResponseDTO>>> GetCurrentPerson()
    {
        var response = await _personService.GetCurrentPerson();
        
        // If person not found, return 404 with JSON response
        if (!response.Success && response.Message?.Contains("Person not found") == true)
        {
            return StatusCode(404, response);
        }
        
        return Ok(response);
    }

    [HttpGet("members")]
    public async Task<ActionResult<BaseResponse<List<PersonResponseDTO>>>> GetMembersByOwner()
    {
        try
        {
            var currentPerson = await _personService.GetCurrentPerson();
            if (currentPerson == null || !currentPerson.Success || currentPerson.Data == null)
                return Unauthorized("User is not authenticated.");

            var ownerId = currentPerson.Data.SupabaseUserId;
            if (string.IsNullOrWhiteSpace(ownerId))
                return Unauthorized("User ID not found");

            var response = await _personService.GetMembersByOwnerAsync(ownerId);
            
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, BaseResponse<List<PersonResponseDTO>>.Fail($"Internal server error: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponse<PersonResponseDTO>>> GetById(string id)
    {
        var response = await _personService.GetMemberByIdAsync(id);
        if (!response.Success || response.Data == null)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BaseResponse<PersonResponseDTO>>> UpdatePerson(
        string id,
        [FromBody] UpdatePersonRequest request)
    {
        if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var personGuid))
        {
            return BadRequest(BaseResponse<PersonResponseDTO>.Fail("Invalid person ID format"));
        }

        var response = await _personService.UpdatePersonAsync(personGuid, request);
        
        if (!response.Success)
            return BadRequest(response);
        
        return Ok(response);
    }
}
