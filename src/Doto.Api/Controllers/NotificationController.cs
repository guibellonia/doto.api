using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Doto.Application.DTOs.Responses;

namespace Doto.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    [HttpGet("test")]
    public ActionResult<BaseResponse<string>> Test()
    {
        return Ok(BaseResponse<string>.Ok("Notifications API is working", "OK"));
    }
}

