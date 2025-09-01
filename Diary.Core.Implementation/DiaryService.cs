using Diary.Core;
using Diary.Data;
using Microsoft.EntityFrameworkCore;
using Entry = Diary.Models.Entry;

namespace Diary.Implementation;

public class DiaryService : IDiaryService
{
    private readonly DiaryDbContext _diaryDbContext;
    private readonly IFileService _fileService;

    public DiaryService(IFileService fileService, DiaryDbContext context)
    {
        _diaryDbContext = context ?? throw new ArgumentNullException(nameof(context));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    public void AddEntry(params Entry[] entries)
    {
        _diaryDbContext.Entries.AddRange(entries.Select(EntryData.FromEntity));
        _diaryDbContext.SaveChanges();
    }

    public Entry? LastEntry()
    {
        return _diaryDbContext.Entries
            .OrderBy(e => e.Time)
            .LastOrDefault()?
            .ToEntity();
    }

    public IEnumerable<Entry> Filter(int? year, int? month, int? day)
    {
        return _diaryDbContext.Entries
            .Where(e => e.Time.Year == (year ?? e.Time.Year)
                && e.Time.Month == (month ?? e.Time.Month)
                && e.Time.Day == (day ?? e.Time.Day))
            .ToList()
            .Select(x => x.ToEntity());
    }

    public IEnumerable<Entry> All()
    {
        return _diaryDbContext.Entries
            .ToList()
            .Select(x => x.ToEntity());
    }

    public IEnumerable<Entry> Search(bool isStrict=false, params string[] args)
    {
        var entries = isStrict
            ? _diaryDbContext.Entries.ToList()
                .Select(e => e.ToEntity())
                .Where(e => args.All(arg => e.Text.Contains(arg, StringComparison.CurrentCulture)))
            : _diaryDbContext.Entries.ToList()
                .Select(e => e.ToEntity())
                .Where(e => args.Any(arg => e.Text.Contains(arg, StringComparison.CurrentCulture)));
        return entries;
    }

    public void Backup(string? name, params object[] args)
    {
        _fileService.Backup(name);
    }

    // TODO: Add export options
}