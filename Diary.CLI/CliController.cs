using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Diary.Core;
using Diary.Core.Exceptions;
using Diary.Models;

namespace Diary.CLI;

public partial class CliController : ICliController
{
    private readonly string _stopWord;
    private readonly float _replaySpeed;
    private readonly IDiaryService _diaryService;
    private readonly IConsoleWrapper _console;
    private readonly Dictionary<string, int?> _months = new()
    {
        { "jan", 1 }, { "feb", 2 }, { "mar", 3 }, { "apr", 4 }, { "may", 5 }, { "jun", 6 },
        { "jul", 7 }, { "aug", 8 }, { "sep", 9 }, { "oct", 10 }, { "nov", 11 }, { "dec", 12 }
    };

    public CliController(IDiaryService diaryService, IOptions<AppConfigs> appConfigs, IConsoleWrapper consoleWrapper)
    {
        _console = consoleWrapper ?? throw new ArgumentNullException(nameof(consoleWrapper));
        _diaryService = diaryService ?? throw new ArgumentNullException(nameof(diaryService));
        if (appConfigs is null) throw new ArgumentNullException(nameof(appConfigs));

        _stopWord = appConfigs.Value.StopWord;
        _replaySpeed = appConfigs.Value.ReplaySpeed;
    }

    public void Log()
    {
        _console.WriteLine(GetPrelogAdvice());
        try
        {
            while (true)
            {
                // TODO: Add locking mechanism / session to avoid other entry points from mixing entries
                var entry = Record();
                _diaryService.AddEntry(entry);
                if (entry.ToString().Contains(_stopWord, StringComparison.InvariantCultureIgnoreCase)) { break; }
            }
        }
        // TODO: Emergency Stop error: clear screen
        catch (Exception)
        {
            _console.WriteLine("Your last entry was broken");
            throw;
        }
    }

    public Entry Record()
    {
        try
        {
            var entry = new Entry { Time = DateTime.Now };
            while (true)
            {
                var stopwatch = Stopwatch.StartNew();
                var chr = _console.ReadKey(true);
                stopwatch.Stop();
                var time = stopwatch.Elapsed.TotalMinutes >= 1
                    ? TimeSpan.FromSeconds(5)
                    : stopwatch.Elapsed;

                if (chr.Key == ConsoleKey.Enter)
                {
                    if (entry.IsEmpty()) { continue; }

                    entry.AddCharacter('\n', (int)time.TotalMilliseconds);
                    _console.WriteLine();
                    return entry;
                }

                // TODO: handle stray characters
                _console.Write(chr.Key == ConsoleKey.Backspace ? "\b \b" : chr.KeyChar);
                entry.AddCharacter(chr.KeyChar, (int)time.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _console.WriteLine(ex.Message);
            throw;
        }
    }

    public void ReplayEntry(Entry entry, float? speed = null)
    {
        speed ??= _replaySpeed;
        var skipFactor = 1;

        if (entry.PrintDate) _console.WriteLine($"\n{entry.Time:ddd yyyy-MMM-dd HH:mm:ss}");

        foreach (var (letter, time) in entry.Text.Zip(entry.Intervals))
        {
            // TODO: wrap thread.sleep
            // TODO: test skip
            Thread.Sleep((int)(time * skipFactor / speed.Value));
            if (_console.KeyAvailable && _console.ReadKey(true).Key is ConsoleKey.Spacebar or ConsoleKey.Enter)
                skipFactor = 0;
            
            _console.Write(letter == '\b' ? "\b \b" : letter);
        }
    }

    public void ReplayEntries(IEnumerable<Entry> entries)
    {
        var entryList = entries.ToList();
        _console.WriteLine($"found {entryList.Count} {(entryList.Count == 1 ? "entry" : "entries")}");

        if (entryList.Count == 0) { return; }
        var lastDateSeen = DateTime.UnixEpoch;
        foreach (var entry in entryList)
        {
            if (entry.Time.Date != lastDateSeen.Date) { entry.PrintDate = true; }
            ReplayEntry(entry);
            lastDateSeen = entry.Time;
        }
    }

    public void ReplayAll()
    {
        var entries = _diaryService.All();
        ReplayEntries(entries);
    }

    public void ReplayToday()
    {
        var today = DateTime.Now;
        var entries = _diaryService.Filter(today.Year, today.Month, today.Day);
        ReplayEntries(entries);
    }

    public void ReplayYesterday()
    {
        var yesterday = DateTime.Now - TimeSpan.FromDays(1);
        var entries = _diaryService.Filter(yesterday.Year, yesterday.Month, yesterday.Day);
        ReplayEntries(entries);
    }

    public void ReplayLast()
    {
        var lastEntry = _diaryService.LastEntry();
        var entries = lastEntry == null
            ? []
            : _diaryService.Filter(lastEntry.Time.Year, lastEntry.Time.Month, lastEntry.Time.Day);
        ReplayEntries(entries);
    }

    public void ReplayFrom(string?[] dates)
    {
        int? year, month, day;
        try
        {
            var yearFound = dates.FirstOrDefault(s => YearRegex().IsMatch(s ?? ""));
            year = yearFound == null ? null : int.Parse(yearFound);

            var monthFound = dates
                .FirstOrDefault(s => MonthRegex().IsMatch(s ?? ""))?[..3]
                ?.ToLowerInvariant();
            month = monthFound == null ? null : _months.GetValueOrDefault(monthFound, null);

            var dayFound = dates.FirstOrDefault(s => DayRegex().IsMatch(s ?? ""));
            day = dayFound == null ? null : int.Parse(dayFound);

            var entries = _diaryService.Filter(year, month, day);
            ReplayEntries(entries);
        }
        catch (Exception)
        {
            _console.WriteLine("That date does not look right");
        }
    }

    public void Search(string[] keywords, bool isStrict = false)
    {
        var entries = _diaryService.Search(isStrict, keywords).ToList();
        foreach (var entry in entries)
        {
            _console.Write($"{entry.Time:yyyy MMM dd HH:mm:ss ddd} | {entry}");
        }
        _console.WriteLine($"{entries.Count} {(entries.Count == 1 ? "entry" : "entries")} found");
    }

    public void Backup(string? name)
    {
        _console.WriteLine($"Backing up diary{(name?.Length > 0 ? name : "...")}");
        _diaryService.Backup(name);
        _console.WriteLine("Backup Complete");
    }

    public void Export(string exportType, string? destination)
    {
        var exportOption = exportType switch
        {
            "txt" => ExportOption.Text,
            // "csv" => ExportOption.Csv,
            // "json" => ExportOption.Json,
            _ => throw new InvalidExportException()
        };
        destination = ProcessExportDestination(destination);
        _diaryService.Export(exportOption, destination);
        _console.WriteLine($"Diary Exported to {destination}.{exportType}");
    }

    /// <summary>
    /// Hidden functionality to convert the python data file to sqlite
    /// </summary>
    /// <param name="filePath"></param>
    public void MigrateDataToNet(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _console.Write("File path / drag-n-drop diary data file: ");
            filePath = _console.ReadLine();
        }

        if (!File.Exists(filePath))
        {
            _console.WriteLine("File does not exist");
            return;
        }

        _console.WriteLine("Migrating diary data to sqlite... ");
        var migratedResource = _diaryService.MigrateDataToNet(filePath);
        _console.WriteLine($"Migrated to {migratedResource}");
        _console.WriteLine($"Move file to 'C:\\Users\\<username>\\AppData\\Roaming\\Diary' for Diary to access.");
    }

    public string GetPrelogAdvice() => $"Ctrl+C or '{_stopWord}' to stop recording.\nSay something memorable about today :)\n";

    public void HandleError(Exception ex)
    {
        switch (ex)
        {
            case InvalidExportException:
                _console.WriteLine("Invalid export type.");
                break;
            case BadFileHeaderException:
                _console.WriteLine("Bad file header found.");
                break;
            default:
                Console.Error.WriteLine(ex.Message);
                break;
        }
    }

    [GeneratedRegex(@"^[a-zA-Z]{3}.*$")]
    private static partial Regex MonthRegex();

    [GeneratedRegex(@"^\d{4}$")]
    private static partial Regex YearRegex();

    [GeneratedRegex(@"^\d{1,2}$")]
    private static partial Regex DayRegex();

    private static string ProcessExportDestination(string? destination)
    {
        destination ??= ".";
        var path = Path.GetFullPath(destination);
        var name = Path.GetFileName(destination);
        var isDirectory = string.IsNullOrEmpty(name) || name == "." || name == "..";
        return isDirectory
            ? Path.Combine(path, $"diaryback_{DateTime.Now:yyyyMMddHHmmss}")
            : path;
    }
}