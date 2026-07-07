using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Doto.Application.DTOs.Requests.MedicineAdherence;
using Doto.Application.DTOs.Responses;
using Doto.Application.Interfaces;

namespace Doto.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MedicineAdherenceController : ControllerBase
{
    private readonly IMedicineAdherenceService _doseOccurrenceService;
    private readonly IPersonService _personService;

    public MedicineAdherenceController(
        IMedicineAdherenceService doseOccurrenceService,
        IPersonService personService)
    {
        _doseOccurrenceService = doseOccurrenceService;
        _personService = personService;
    }

    [HttpPost("{id:guid}/taken")]
    public async Task<ActionResult<BaseResponse<bool>>> MarkTaken(
        Guid id, 
        [FromBody] MarkDoseTakenRequest body,
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

        if (body == null)
            return BadRequest(BaseResponse<bool>.Fail("Request body cannot be null", false));

        try
        {
            var result = await _doseOccurrenceService.MarkDoseTakenAsync(id, body.TakenAt, effectivePersonId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return BadRequest(BaseResponse<bool>.Fail($"Invalid date format: {ex.Message}", false));
        }
        catch (System.ArgumentOutOfRangeException ex)
        {
            return BadRequest(BaseResponse<bool>.Fail($"Date value out of bounds: {ex.Message}", false));
        }
        catch (Exception ex)
        {
            return BadRequest(BaseResponse<bool>.Fail($"Error processing request: {ex.Message}", false));
        }
    }

    [HttpPost("{id:guid}/skip")]
    public async Task<ActionResult<BaseResponse<bool>>> MarkSkipped(
        Guid id, 
        [FromBody] MarkDoseSkippedRequest body,
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

        var result = await _doseOccurrenceService.MarkDoseSkippedAsync(id, body.Reason, effectivePersonId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/snooze")]
    public async Task<ActionResult<BaseResponse<bool>>> Snooze(
        Guid id, 
        [FromBody] SnoozeDoseRequest body,
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

        var result = await _doseOccurrenceService.SnoozeDoseAsync(id, body.DelayInMinutes, effectivePersonId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("history/daily")]
    public async Task<ActionResult<BaseResponse<IReadOnlyList<DoseOccurrenceDto>>>> GetDailyHistory(
        [FromQuery] DateOnly date,
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

        var result = await _doseOccurrenceService.GetDailyHistoryAsync(date, effectivePersonId);
        return Ok(result);
    }

    [HttpGet("history/monthly")]
    public async Task<ActionResult<BaseResponse<IReadOnlyList<DoseOccurrenceDto>>>> GetMonthlyHistory(
        [FromQuery] int year,
        [FromQuery] int month,
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

        var result = await _doseOccurrenceService.GetMonthlyHistoryAsync(year, month, effectivePersonId);
        return Ok(result);
    }

    [HttpGet("by-medicine-schedule-date")]
    public async Task<ActionResult<BaseResponse<DoseOccurrenceDto?>>> GetByMedicineScheduleAndDate(
        [FromQuery] Guid medicineId,
        [FromQuery] Guid scheduleId,
        [FromQuery] DateOnly date,
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

        var result = await _doseOccurrenceService.GetDoseOccurrenceByMedicineScheduleAndDateAsync(medicineId, scheduleId, date, effectivePersonId);
        return Ok(result);
    }
}
