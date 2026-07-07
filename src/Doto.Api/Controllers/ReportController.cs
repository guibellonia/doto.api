using Microsoft.AspNetCore.Mvc;
using Doto.Application.DTOs;
using Doto.Application.DTOs.Requests.Report;
using Doto.Application.DTOs.Responses;
using Doto.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;

namespace Doto.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IPersonService _personService;

    public ReportController(
        IReportService reportService,
        IPersonService personService)
    {
        _reportService = reportService;
        _personService = personService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<BaseResponse<ReportDtoResponse>>> GenerateReport(
        [FromBody] GenerateReportRequest request,
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

        var response = await _reportService.GenerateReportAsync(effectivePersonId, request);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}

