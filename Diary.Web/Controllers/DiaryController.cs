using Diary.Core;
using Microsoft.AspNetCore.Mvc;

namespace Diary.Web.Controllers;

[ApiController]
[Route("diary")]
public class DiaryController : ControllerBase
{
    private readonly ILogger<DiaryController> _logger;
    private readonly IDiaryService _diaryService;

    public DiaryController(ILogger<DiaryController> logger, IDiaryService diaryService)
    {
        _logger = logger;
        _diaryService = diaryService;
    }

    [HttpGet("all")]
    public ActionResult<IEnumerable<Contracts.Entry>> GetAllEntries()
    {
        _logger.LogInformation("Fetching all diary entries.");
        var entries = _diaryService.All()
            .Select(Contracts.Entry.FromEntity);
        return Ok(entries);
    }
}