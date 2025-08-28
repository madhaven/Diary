using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Diary.Core;
using Diary.Models;

namespace Diary.CLI;

public partial class CliController : ICliController // TODO: remove partial
{
    private readonly string _stopWord;
    private readonly float _replaySpeed;
    private readonly IDiaryService _diaryService;

    public CliController(IDiaryService diaryService, IOptions<AppConfigs> appConfigs)
    {
        _diaryService = diaryService ?? throw new ArgumentNullException(nameof(diaryService));
        _stopWord = appConfigs.Value.StopWord;
        _replaySpeed = appConfigs.Value.ReplaySpeed;
    }

    public void Log()
    {
        Console.WriteLine(GetPrelogAdvice());
        try
        {
            while (true)
            {
                var entry = Record();
                _diaryService.AddEntry(entry);
                if (entry.ToString().Contains(_stopWord, StringComparison.InvariantCultureIgnoreCase)) { break; }
            }
        }
        // TODO: Emergency Stop error: clear screen
        catch (Exception)
        {
            Console.WriteLine("Your last entry was broken");
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
                var chr = Console.ReadKey(true);
                stopwatch.Stop();
                var time = stopwatch.Elapsed.TotalMinutes >= 1
                    ? TimeSpan.FromSeconds(5)
                    : stopwatch.Elapsed;

                if (chr.Key == ConsoleKey.Enter)
                {
                    if (entry.IsEmpty()) { continue; }

                    entry.AddCharacter('\n', (int)time.TotalMilliseconds);
                    Console.WriteLine();
                    return entry;
                }

                if (chr is { Key: ConsoleKey.C, Modifiers: ConsoleModifiers.Control })
                {
                    if (!entry.IsEmpty())
                    {
                        entry.AddCharacter('\n', (int)time.TotalMilliseconds);
                    }

                    throw new Exception("EMERGENCY STOP ERROR"); // TODO
                }

                Console.Write(chr.Key == ConsoleKey.Backspace ? "\b \b" : chr.KeyChar);
                // TODO: handle stray characters
                entry.AddCharacter(chr.KeyChar, (int)time.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public void ReplayEntry(Entry entry, float? speed = null)
    {
        speed ??= _replaySpeed;
        var skipFactor = 1;

        if (entry.PrintDate) Console.WriteLine($"\n{entry.Time.ToString(CultureInfo.InvariantCulture)}");

        foreach (var (letter, time) in entry.Text.Zip(entry.Intervals))
        {
            Thread.Sleep((int)(time * skipFactor / speed.Value));
            if (Console.KeyAvailable && Console.ReadKey(true).Key is ConsoleKey.Spacebar or ConsoleKey.Enter)
                skipFactor = 0;
            
            Console.Write(letter == '\b' ? "\b \b" : letter);
        }
    }

    public void ReplayEntries(IEnumerable<Entry> entries)
    {
        var entryList = entries.ToList();
        Console.WriteLine($"found {entryList.Count} {(entryList.Count == 1 ? "entry" : "entries")}");
        foreach (var entry in entryList) { ReplayEntry(entry); }
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

    public void ReplayFrom(List<string?> dates)
    {
        int? year, month, day;
        try
        {
            var yearFound = dates.FirstOrDefault(s => Regex.IsMatch(s ?? "", @"^\d{4}$"));
            year = yearFound == null ? null : int.Parse(yearFound);

            var monthFound = dates
                .FirstOrDefault(s => Regex.IsMatch(s ?? "", @"^[a-zA-Z]{3}.*$"))?[..3]
                ?.ToLowerInvariant();
            var months = new Dictionary<string, int?> {
                { "jan", 1 }, { "feb", 2 }, { "mar", 3 }, { "apr", 4 }, { "may", 5 }, { "jun", 6 },
                { "jul", 7 }, { "aug", 8 }, { "sep", 9 }, { "oct", 10 }, { "nov", 11 }, { "dec", 12 } }; 
            month = monthFound == null ? null : months.GetValueOrDefault(monthFound, null);

            var dayFound = dates.FirstOrDefault(s => Regex.IsMatch(s ?? "", @"^\d{1,2}$"));
            day = dayFound == null ? null : int.Parse(dayFound);

            var entries = _diaryService.Filter(year, month, day);
            ReplayEntries(entries);
        }
        catch (Exception)
        {
            Console.WriteLine("That date does not look right");
        }
    }

    public void Search(string[] keywords, bool isStrict = false)
    {
        var entries = _diaryService.Search(isStrict, keywords).ToList();
        foreach (var entry in entries)
        {
            var timeString = entry.Time.ToString("yyyy-MM-dd HH:mm:ss ddd");
            Console.Write($"{timeString} | {entry}");
        }
        Console.WriteLine($"{entries.Count} {(entries.Count == 1 ? "entry" : "entries")} found");
    }

    public void Backup(string? name)
    {
        Console.WriteLine($"Backing up diary{(name?.Length > 0 ? name : "...")}");
        _diaryService.Backup(name);
        Console.WriteLine("Backup Complete");
    }

    public string GetPrelogAdvice() => $"Ctrl+C or '{_stopWord}' to stop recording.\nSay something memorable about today :)\n";
}