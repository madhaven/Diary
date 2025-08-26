namespace Diary.Core;

public class DiaryService : IDiaryService
{
    private readonly IFileService _fileService;
    
    public DiaryService(IFileService fileService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    public void AddEntry(params Entry[] entries)
    {
        _fileService.Write(entries);
    }

    public IEnumerable<Entry> FetchEntries()
    {
        return _fileService.Load();
    }

    public IEnumerable<Entry> Filter(int? year, int? month, int? day)
    {
        return FetchEntries().Where(e => e.Time.Year == (year ?? e.Time.Year)
            && e.Time.Month == (month ?? e.Time.Month)
            && e.Time.Day == (day ?? e.Time.Day));
    }

    public IEnumerable<Entry> All()
    {
        return FetchEntries();
    }

    public IEnumerable<Entry> Search(bool isStrict=false, params string[] args)
    {
        var entries = FetchEntries();
        var results = isStrict
            ? entries.Where(e => args.All(
                arg => e.ToString().Contains(arg, StringComparison.CurrentCultureIgnoreCase)))
            : entries.Where(e => args.Any(
                arg => e.ToString().Contains(arg, StringComparison.CurrentCultureIgnoreCase)));
        return results;
    }

    public void Backup(string? name, params object[] args)
    {
        _fileService.Backup(name);
    }

    // TODO: Add export options
}