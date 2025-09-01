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
        var entryEntities = entries.Select(EntryData.FromEntity);
        _diaryDbContext.Entries.AddRange(entryEntities);
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
            .AsEnumerable()
            .Select(x => x.ToEntity());
    }

    public IEnumerable<Entry> All()
    {
        return _diaryDbContext.Entries
            .AsEnumerable()
            .Select(x => x.ToEntity());
    }

    public IEnumerable<Entry> Search(bool isStrict=false, params string[] args)
    {
        var entries = isStrict
            ? _diaryDbContext.Entries.AsEnumerable()
                .Select(e => e.ToEntity())
                .Where(e => args.All(arg => e.Text.Contains(arg, StringComparison.CurrentCulture)))
            : _diaryDbContext.Entries.AsEnumerable()
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