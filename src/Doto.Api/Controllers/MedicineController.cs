using Microsoft.AspNetCore.Mvc;
using Doto.Application.DTOs;
using Doto.Application.DTOs.Requests.Medicine;
using Doto.Application.DTOs.Responses;
using Doto.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;

namespace Doto.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MedicineController : ControllerBase
{
    private readonly IMedicineService _medicineService;
    private readonly IPersonService _personService;

    public MedicineController(
        IMedicineService medicineService,
        IPersonService personService
        )
    {
        _medicineService = medicineService;
        _personService = personService;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<MedicineDtoResponse>>>> GetAll(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
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

        var response = await _medicineService.GetAllByPersonAsync(effectivePersonId, page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BaseResponse<MedicineDtoResponse?>>> GetById(
        Guid id,
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

        var response = await _medicineService.GetByIdAsync(effectivePersonId, id);
        if (response == null || response.Data == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse<MedicineDtoResponse>>> Create(
        [FromBody] CreateMedicineRequest request,
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

        var created = await _medicineService.AddMedicineAsync(effectivePersonId, request);
        return CreatedAtAction(nameof(GetById), new { id = created.Data?.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BaseResponse<MedicineDtoResponse>>> Update(
        Guid id, 
        [FromBody] UpdateMedicineRequest request,
        [FromQuery] string? memberId = null)
    {
        if (id != request.Id)
            return BadRequest("The ID in the URL does not match the ID in the request body.");

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

        var updated = await _medicineService.UpdateMedicineAsync(effectivePersonId, request);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<BaseResponse<bool>>> SoftDelete(
        Guid id,
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

        var response = await _medicineService.SoftDeleteAsync(effectivePersonId, id);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
