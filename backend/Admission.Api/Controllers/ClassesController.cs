using Admission.Api.Dtos;
using Admission.Api.Helpers;
using Admission.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admission.Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController : ControllerBase
{
    private readonly ClassService _classService;
    private readonly HoldService _holdService;

    public ClassesController(ClassService classService, HoldService holdService)
    {
        _classService = classService;
        _holdService = holdService;
    }

    // POST /classes
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request)
    {
        var created = await _classService.CreateClass(request);
        return Created($"/classes/{created.Id}", created);
    }

    // GET /classes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _classService.GetAllClasses());
    }

    // GET /classes/{classId}
    [HttpGet("{classId}")]
    public async Task<IActionResult> GetById(string classId)
    {
        return Ok(await _classService.GetClass(classId));
    }

    // POST /classes/{classId}/holds  -> apply for a seat
    [HttpPost("{classId}/holds")]
    public async Task<IActionResult> CreateHold(string classId)
    {
        var studentId = StudentIdHelper.ReadFromHeader(Request);
        var hold = await _holdService.CreateHold(classId, studentId);
        return Created($"/holds/{hold.HoldId}", hold);
    }
}
