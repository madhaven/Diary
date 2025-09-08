using Diary.Core;
using Diary.Core.Exceptions;
using Diary.Data;
using Diary.Models;
using Microsoft.EntityFrameworkCore;
using Entry = Diary.Models.Entry;

namespace Diary.Implementation;

public class DiaryService : IDiaryService
{
    private readonly DiaryDbContext _diaryDbContext;
    private readonly IFileService _fileService;
    private readonly IExportStrategyFactory _exportStrategyFactory;

    public DiaryService(IFileService fileService, DiaryDbContext context, IExportStrategyFactory exportStrategyFactory)
    {
        _diaryDbContext = context ?? throw new ArgumentNullException(nameof(context));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _exportStrategyFactory = exportStrategyFactory ?? throw new ArgumentNullException(nameof(exportStrategyFactory));
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

    public IEnumerable<Entry> Search(bool isStrict = false, params string[] args)
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

    public void Export(ExportOption exportOption, string destination)
    {
        var strategy = _exportStrategyFactory.CreateExporter(exportOption);
        var entries = _diaryDbContext.Entries
            .Select(e => e.ToEntity())
            .ToList();
        strategy.Export(entries, destination);
    }

    public void MigrateDataToNet(string filePath)
    {
        // setup db
        string sqlitePath = Path.GetFileNameWithoutExtension(filePath) + ".sqlite";
        while (File.Exists(sqlitePath))
            sqlitePath = $".{sqlitePath}";
        var dbOptions = new DbContextOptionsBuilder<DiaryDbContext>()
            .UseSqlite($"Data Source={sqlitePath};")
            .Options;
        var newDbContext = new DiaryDbContext(dbOptions);

        // setup files
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var streamReader = new StreamReader(fileStream);

        List<Entry> entries = [];
        var firstLine = streamReader.ReadLine();
        if (firstLine != "diary v3.2 github.com/madhaven/diary" && firstLine != "diary v2.10 github.com/madhaven/diary")
            throw new BadFileHeaderException();
        
        // process file
        while (true)
        {
            streamReader.ReadLine();
            var timeAndText = streamReader.ReadLine();
            if (timeAndText == null)
                break;

            var text = timeAndText[24..] + '\n';
            var time = DateTime.ParseExact(
                timeAndText.Substring(0, 24).Replace("  ", " 0"),
                "ddd MMM dd HH:mm:ss yyyy",
                System.Globalization.CultureInfo.InvariantCulture);
            var intervals = streamReader.ReadLine()?[1..^1]?
                .Split(',').Select(x => double.Parse(x) * 1000).ToList()
                ?? Enumerable.Repeat(0d, timeAndText.Length).ToList();

            var entry = new Entry
            {
                Text = text,
                Time = time,
                Intervals = intervals,
                PrintDate = entries.Count > 0 && entries[^1].Time.Date != time.Date
            };

            entries.Add(entry);
        }

        // add to db
        var entryData = entries.Select(EntryData.FromEntity);
        newDbContext.Database.Migrate();
        newDbContext.Entries.AddRange(entryData);
        newDbContext.SaveChanges();
    }
}
