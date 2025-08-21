using System.CommandLine;
using System.Diagnostics;

namespace Diary.CLI;

public class CliController
{
    private string _stopWord;
    private float _replaySpeed;
    private readonly Diary _diary;
    
    public CliController(Diary diary, string stopWord, float replaySpeed)
    {
        _diary = diary ?? throw new ArgumentNullException(nameof(diary));
        _stopWord = stopWord ?? throw new ArgumentNullException(nameof(stopWord));
        _replaySpeed = replaySpeed;
    }

    public void ShowVersion(){}
    
    public void ShowUsage()
    {
        ShowVersion();
        Console.WriteLine("\nUsage");
        Console.WriteLine("-----");
        Console.WriteLine("diary log|entry - to add to your diary");
        Console.WriteLine("diary version - to check which version your diary is running");
        Console.WriteLine("diary read - to access older entries or logs that you have made");
        Console.WriteLine("diary search|find - to search for keywords");
        Console.WriteLine("diary searchall|findall - to search for entries containing all the keywords");
        Console.WriteLine("diary backup [filename]");
        Console.WriteLine("diary export - to export your entries to portable formats | NOT AVAILABLE");
    }
    
    /// <summary>
    /// Records diary entries
    /// Initiates a loop of Entry recordings
    /// Loop ends when the stop word is found in an Entry
    /// </summary>
    public void Log(Diary diary)
    {
        try
        {
            while (true)
            {
                var entry = Record();
                diary.AddEntry(entry);
                if (entry.ToString().ToLowerInvariant().Contains(_stopWord))
                {
                    break;
                }
            }
        }
        // TODO: Emergency Stop error: clear screen
        catch (Exception)
        {
            Console.WriteLine("Your last entry was broken");
            throw;
        }
    }
    
    /// <summary>
    /// method to record an entry from the cli interface.
    /// A single entry ends when the return key is pressed.
    /// The diary log ends if the stopword is found in the entry.
    /// </summary>
    /// <returns>Entry object</returns>
    public Entry Record()
    {
        try
        {
            var entry = new Entry { Time = DateTime.Now };
            var t1 = DateTime.Now;
            while (true)
            {
                var chr = Console.ReadKey(true);
                var t2 = DateTime.Now;
                TimeSpan t3 = t2.Subtract(t1).TotalMinutes >= 1
                    ? t3 = TimeSpan.FromSeconds(5)
                    : t3 = t2 - t1;

                if (chr.Key == ConsoleKey.Enter)
                {
                    if (entry.IsEmpty())
                    {
                        continue;
                    }

                    entry.AddCharacter('\n', t3);
                    Console.WriteLine();
                    return entry;
                }

                if (chr is { Key: ConsoleKey.C, Modifiers: ConsoleModifiers.Control })
                {
                    if (!entry.IsEmpty())
                    {
                        entry.AddCharacter('\n', t3);
                    }

                    throw new Exception("EMERGENCY STOP ERROR"); // TODO
                }

                Console.Write(chr.Key == ConsoleKey.Backspace ? "\b \b" : chr.KeyChar);
                // TODO: handle stray characters
                entry.AddCharacter(chr.KeyChar, t3);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// replays the entry as it was typed in, imitating the user's typespeed.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="speed"></param>
    public void ReplayEntry(Entry entry, float? speed)
    {
        speed ??= _replaySpeed;
        var skipFactor = 1;

        if (entry.PrintDate)
        {
            Console.WriteLine($"\n{entry.Time.ToString()}");
        }

        foreach (var (letter, time) in entry.Text.Zip(entry.Intervals))
        {
            Thread.Sleep((int)(time.Milliseconds * skipFactor / speed.Value));
            if (Console.KeyAvailable &&
                    Console.ReadKey(true) is { Key: ConsoleKey.Enter | ConsoleKey.Escape | ConsoleKey.Spacebar })
            {
                skipFactor = 0;
            }
            
            Console.Write(letter == '\b' ? "\b \b" : letter);
        }
    }

    public void Backup(string? name)
    {
        Console.WriteLine($"Backing up diary{(name?.Length > 0 ? name : "...")}");
        _diary.Backup(name);
        Console.WriteLine("Backup Complete");
    }

    public string GetPrelogAdvice() => "Ctrl+C / 'bye' to stop recording.\nSay something memorable about today :)\n";
}