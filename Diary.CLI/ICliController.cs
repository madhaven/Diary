using Diary.Models;

namespace Diary.CLI;

public interface ICliController
{

    /// <summary>
    /// Records diary entries
    /// Initiates a loop of Entry recordings
    /// Loop ends when the stop word is found in an Entry
    /// </summary>
    public void Log();

    /// <summary>
    /// method to record an entry from the cli interface.
    /// A single entry ends when the return key is pressed.
    /// The diary log ends if the stopword is found in the entry.
    /// </summary>
    /// <returns>Entry object</returns>
    public Entry Record();

    /// <summary>
    /// replays the entry as it was typed in, imitating the user's typespeed.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="speed"></param>
    public void ReplayEntry(Entry entry, float? speed = null);

    public void ReplayEntries(IEnumerable<Entry> entries);

    public void ReplayAll();

    public void ReplayToday();

    public void ReplayYesterday();

    public void ReplayFrom(List<string?> dates);

    public void Search(string[] keywords, bool isStrict = false);

    public void Backup(string? name);

    public string GetPrelogAdvice();
}