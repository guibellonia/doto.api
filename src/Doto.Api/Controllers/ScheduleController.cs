using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Doto.Application.DTOs;
using Doto.Application.DTOs.Requests.Schedule;
using Doto.Application.DTOs.Responses;
using Doto.Application.Interfaces;

namespace Doto.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;
    private readonly IPersonService _personService;

    public ScheduleController(IScheduleService scheduleService, IPersonService personService)
    {
        _scheduleService = scheduleService;
        _personService = personService;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<ScheduleDtoResponse>>>> GetAll(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? memberId = null,
        [FromQuery] DateOnly? date = null)
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

        var response = await _scheduleService.GetAllByPersonAsync(effectivePersonId, page, pageSize, date);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BaseResponse<ScheduleDtoResponse?>>> GetById(
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

        var schedule = await _scheduleService.GetByIdAsync(effectivePersonId, id);
        if (schedule == null || schedule.Data == null)
            return NotFound();

        return Ok(schedule);
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse<ScheduleDtoResponse>>> Create(
        [FromBody] CreateScheduleRequest request,
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

        var created = await _scheduleService.AddScheduleAsync(effectivePersonId, request);
        return CreatedAtAction(nameof(GetById), new { id = created.Data?.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BaseResponse<ScheduleDtoResponse>>> Update(
        Guid id, 
        [FromBody] UpdateScheduleRequest request,
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

        var updated = await _scheduleService.UpdateScheduleAsync(effectivePersonId, request);
        return Ok(updated);
    }
}