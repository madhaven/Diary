using Diary.Core;
using Diary.Implementation;
using Diary.Web.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Diary.Web.Controllers;

[ApiController]
[Route("api")]
public class DiaryController : ControllerBase
{
    private readonly ILogger<DiaryController> _logger;
    private readonly IDiaryService _diaryService;

    public DiaryController(ILogger<DiaryController> logger, IDiaryService diaryService)
    {
        _logger = logger;
        _diaryService = diaryService;
    }

    /// <summary>
    /// Finds Entries based on Search Request object which specifies the query strings and isStrict parameter
    /// </summary>
    /// <param name="request">SearchRequest object</param>
    /// <returns>List of entries matching the queries</returns>
    [HttpGet("entry")]
    public ActionResult<IEnumerable<Entry>> FindEntries([FromBody] SearchRequest request)
    {
        _logger.LogInformation("Finding entries");
        var entries = _diaryService.Search(request.IsStrict, request.Queries)
            .Select(Entry.FromEntity);
        return Ok(entries);

    }

    /// <summary>
    /// Gets all Diary Entries
    /// </summary>
    /// <returns>list of all entries</returns>
    [HttpGet("entry/all")]
    public ActionResult<IEnumerable<Entry>> GetAllEntries()
    {
        // TODO: add pagination
        _logger.LogInformation("Fetching all diary entries.");
        var entries = _diaryService.All()
            .Select(Entry.FromEntity);
        return Ok(entries);
    }

    /// <summary>
    /// Create an Entry
    /// </summary>
    /// <param name="entry">entry</param>
    /// <returns>created entry</returns>
    [HttpPost("entry")]
    public ActionResult<Entry> AddEntry(Entry entry)
    {
        _logger.LogInformation("Adding Entry.");
        var entity = entry.ToEntity();
        _diaryService.AddEntry(entity);
        return Ok(entity);
    }

    /// <summary>
    /// Get a single entry
    /// </summary>
    /// <param name="id">entry id</param>
    /// <returns>Entry</returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet("entry/{id}")]
    public ActionResult<Entry> GetEntry([FromRoute] int id)
    {
        throw new NotImplementedException();
    }
}