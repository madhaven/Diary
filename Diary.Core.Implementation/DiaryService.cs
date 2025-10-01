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
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var streamReader = new StreamReader(fileStream);
        List<Entry> entries = [];

        var firstLine = streamReader.ReadLine();
        if (firstLine == null || firstLine != "diary v2.10 github.com/madhaven/diary")
            throw new BadFileHeaderException();
        
        // setup db
        string sqlitePath = Path.GetFileNameWithoutExtension(filePath) + ".sqlite";
        if (File.Exists(sqlitePath)) // TODO: handle with exceptions instead of overhead logic
            sqlitePath = Path.GetFileNameWithoutExtension(filePath) + "_" + Path.GetRandomFileName() + ".sqlite";
        var dbOptions = new DbContextOptionsBuilder<DiaryDbContext>()
            .UseSqlite($"Data Source={sqlitePath};")
            .Options;
        var newDbContext = new DiaryDbContext(dbOptions);
        newDbContext.Database.Migrate();

        // process file
        int timestampLength = 24;
        var expectedFormat = "ddd MMM dd HH:mm:ss yyyy";
        using var transaction = newDbContext.Database.BeginTransaction();
        while (true)
        {
            var emptyLine = streamReader.ReadLine();
            if (emptyLine == null) break;

            var timeAndText = streamReader.ReadLine();
            if (timeAndText == null) break;

            var text = timeAndText[timestampLength..] + '\n';
            var time = DateTime.ParseExact(
                timeAndText.Substring(0, timestampLength).Replace("  ", " 0"),
                expectedFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None);
            
            var intervalsLine = streamReader.ReadLine();
            var intervals = intervalsLine?[1..^1]?.Split(',').Select(x =>
            {
                if (!double.TryParse(x, out var interval))
                {
                    throw new BadFileHeaderException();
                }
                return interval * 1000;
            }).ToList() ?? Enumerable.Empty<double>().ToList();

            var entry = new Entry
            {
                Text = text,
                Time = time,
                Intervals = intervals,
                PrintDate = entries.Count > 0 && entries[^1].Time.Date != time.Date
            };

            entries.Add(entry);
        }
        
        newDbContext.Entries.AddRange(entries.Select(EntryData.FromEntity));
        newDbContext.SaveChanges();
        transaction.Commit();
    }
}
