using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Doto.Application.DTOs.Requests.Health;
using Doto.Application.DTOs.Responses;
using Doto.Application.Helpers;
using Doto.Application.Interfaces;
using Doto.Domain.Interfaces;

namespace Doto.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;
    private readonly IPersonService _personService;

    public HealthController(
        IHealthService healthService,
        IPersonService personService)
    {
        _healthService = healthService;
        _personService = personService;
    }

    [HttpPost("blood-pressure")]
    public async Task<ActionResult<BaseResponse<VitalSignRecordDto>>> RegisterBloodPressure(
        [FromBody] RegisterBloodPressureRequest request,
        [FromQuery] string? memberId = null)
    {
        var person = await _personService.GetCurrentPerson();
        if (person == null || person.Data == null)
            return Unauthorized("User is not authenticated.");

        Guid effectivePersonId;
        try
        {
            effectivePersonId = await _personService.GetEffectivePersonIdAsync(memberId);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }

        var result = await _healthService.RegisterBloodPressureAsync(effectivePersonId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPost("blood-sugar")]
    public async Task<ActionResult<BaseResponse<VitalSignRecordDto>>> RegisterBloodSugar(
        [FromBody] RegisterBloodSugarRequest request,
        [FromQuery] string? memberId = null)
    {
        var person = await _personService.GetCurrentPerson();
        if (person == null || person.Data == null)
            return Unauthorized("User is not authenticated.");

        Guid effectivePersonId;
        try
        {
            effectivePersonId = await _personService.GetEffectivePersonIdAsync(memberId);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }

        var result = await _healthService.RegisterBloodSugarAsync(effectivePersonId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPost("weight")]
    public async Task<ActionResult<BaseResponse<VitalSignRecordDto>>> RegisterWeight(
        [FromBody] RegisterWeightRequest request,
        [FromQuery] string? memberId = null)
    {
        var person = await _personService.GetCurrentPerson();
        if (person == null || person.Data == null)
            return Unauthorized("User is not authenticated.");

        Guid effectivePersonId;
        try
        {
            effectivePersonId = await _personService.GetEffectivePersonIdAsync(memberId);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }

        var result = await _healthService.RegisterWeightAsync(effectivePersonId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPost("height")]
    public async Task<ActionResult<BaseResponse<VitalSignRecordDto>>> RegisterHeight(
        [FromBody] RegisterHeightRequest request,
        [FromQuery] string? memberId = null)
    {
        var person = await _personService.GetCurrentPerson();
        if (person == null || person.Data == null)
            return Unauthorized("User is not authenticated.");

        Guid effectivePersonId;
        try
        {
            effectivePersonId = await _personService.GetEffectivePersonIdAsync(memberId);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }

        var result = await _healthService.RegisterHeightAsync(effectivePersonId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPost("symptoms")]
    public async Task<ActionResult<BaseResponse<SymptomRecordDto>>> RegisterSymptom(
        [FromBody] RegisterSymptomRequest request,
        [FromQuery] string? memberId = null)
    {
        var person = await _personService.GetCurrentPerson();
        if (person == null || person.Data == null)
            return Unauthorized("User is not authenticated.");

        Guid effectivePersonId;
        try
        {
            effectivePersonId = await _personService.GetEffectivePersonIdAsync(memberId);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }

        var result = await _healthService.RegisterSymptomAsync(effectivePersonId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpGet("vital-signs")]
    public async Task<ActionResult<BaseResponse<IReadOnlyList<VitalSignRecordDto>>>> GetVitalSigns(
        [FromQuery] string? memberId = null)
    {
        var person = await _personService.GetCurrentPerson();
        if (person == null || person.Data == null)
            return Unauthorized("User is not authenticated.");

        Guid effectivePersonId;
        try
        {
            effectivePersonId = await _personService.GetEffectivePersonIdAsync(memberId);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }

        var result = await _healthService.GetVitalSignsAsync(effectivePersonId);
        return Ok(result);
    }

    [HttpGet("vital-signs/latest")]
    public async Task<ActionResult<BaseResponse<VitalSignRecordDto?>>> GetLatestVitalSign(
        [FromQuery] int type,
        [FromQuery] string? memberId = null)
    {
        var person = await _personService.GetCurrentPerson();
        if (person == null || person.Data == null)
            return Unauthorized("User is not authenticated.");

        Guid effectivePersonId;
        try
        {
            effectivePersonId = await _personService.GetEffectivePersonIdAsync(memberId);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }

        var result = await _healthService.GetLatestVitalSignByTypeAsync(effectivePersonId, type);
        return Ok(result);
    }

    [HttpGet("symptoms")]
    public async Task<ActionResult<BaseResponse<IReadOnlyList<SymptomRecordDto>>>> GetSymptoms(
        [FromQuery] string? memberId = null)
    {
        var person = await _personService.GetCurrentPerson();
        if (person == null || person.Data == null)
            return Unauthorized("User is not authenticated.");

        Guid effectivePersonId;
        try
        {
            effectivePersonId = await _personService.GetEffectivePersonIdAsync(memberId);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }

        var result = await _healthService.GetSymptomsAsync(effectivePersonId);
        return Ok(result);
    }
}

