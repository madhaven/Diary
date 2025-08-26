namespace Diary.Core;

public class Diary
{
    // TODO: consider returning diary object after each operation
    private readonly IFileService _fileService;
    private List<Entry> _entries;
    
    /// <summary>
    /// initializes the Diary
    /// </summary>
    /// <param name="fileService"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Diary(IFileService fileService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _entries = [];
    }

    /// <summary>
    /// writes the entry/entries to the file
    /// </summary>
    /// <param name="entries"></param>
    public void AddEntry(params Entry[] entries)
    {
        _fileService.Write(entries);
    }

    /// <summary>
    /// Scans the file specified on creation and initializes the list of Entry objects
    /// This replaces any existing Entries in memory with data from the file
    ///
    /// Only recommended when reading entries as adding entries to the diary do not require any data in memory  
    /// </summary>
    public void Load()
    {
        _entries = _fileService.Load().ToList();
    }

    /// <summary>
    /// fetches diary records acc to date match, if `fetchLatest` is set to True.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <returns></returns>
    public IEnumerable<Entry> Filter(int? year, int? month, int? day)
    {
        Load();
        return _entries.Where(e => e.Time.Year == (year ?? e.Time.Year)
            && e.Time.Month == (month ?? e.Time.Month)
            && e.Time.Day == (day ?? e.Time.Day));
    }

    public IEnumerable<Entry> All()
    {
        Load();
        return _entries;
    }

    public IEnumerable<Entry> Search(bool isStrict=false, params string[] args)
    {
        Load();
        var results = isStrict
            ? _entries.Where(e => args.All(
                arg => e.ToString().Contains(arg, StringComparison.CurrentCultureIgnoreCase)))
            : _entries.Where(e => args.Any(
                arg => e.ToString().Contains(arg, StringComparison.CurrentCultureIgnoreCase)));
        return results;
    }

    public void Backup(string? name, params object[] args)
    {
        _fileService.Backup(name);
    }

    // TODO: Add export options
}