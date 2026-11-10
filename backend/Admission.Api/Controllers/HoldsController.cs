using Admission.Api.Helpers;
using Admission.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admission.Api.Controllers;

[ApiController]
public class HoldsController : ControllerBase
{
    private readonly HoldService _holdService;

    public HoldsController(HoldService holdService)
    {
        _holdService = holdService;
    }

    // GET /holds/{holdId}
    [HttpGet("holds/{holdId}")]
    public async Task<IActionResult> GetById(string holdId)
    {
        return Ok(await _holdService.GetHold(holdId));
    }

    // POST /holds/{holdId}/confirm  -> pay admission fee
    [HttpPost("holds/{holdId}/confirm")]
    public async Task<IActionResult> Confirm(string holdId)
    {
        var studentId = StudentIdHelper.ReadFromHeader(Request);
        return Ok(await _holdService.ConfirmHold(holdId, studentId));
    }

    // DELETE /holds/{holdId}  -> cancel application
    [HttpDelete("holds/{holdId}")]
    public async Task<IActionResult> Cancel(string holdId)
    {
        var studentId = StudentIdHelper.ReadFromHeader(Request);
        await _holdService.CancelHold(holdId, studentId);
        return NoContent();
    }

    // GET /me/holds  -> all applications of the current student
    [HttpGet("me/holds")]
    public async Task<IActionResult> GetMyHolds()
    {
        var studentId = StudentIdHelper.ReadFromHeader(Request);
        return Ok(await _holdService.GetStudentHolds(studentId));
    }
}
