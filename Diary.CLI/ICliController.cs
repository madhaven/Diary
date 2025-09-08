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

    /// <summary>
    /// Replays a list of entries.
    /// </summary>
    /// <param name="entries"></param>
    public void ReplayEntries(IEnumerable<Entry> entries);

    /// <summary>
    /// Replays all the entries available.
    /// </summary>
    public void ReplayAll();

    /// <summary>
    /// Replay entries made today.
    /// </summary>
    public void ReplayToday();

    /// <summary>
    /// Replay entries made yesterday.
    /// </summary>
    public void ReplayYesterday();

    /// <summary>
    /// Replay entries from the day the last entry was made.
    /// </summary>
    public void ReplayLast();

    /// <summary>
    /// Replay entries from a specific combination of year/month/date.
    /// </summary>
    /// <param name="dates">at least one year/month/date specification is required.</param>
    public void ReplayFrom(string?[] dates);

    /// <summary>
    /// Searches for the keywords in the diary.
    /// </summary>
    /// <param name="keywords">keywords for the search</param>
    /// <param name="isStrict">forces all keywords to be present in the results</param>
    public void Search(string[] keywords, bool isStrict = false);

    /// <summary>
    /// Backs up diary.
    /// </summary>
    /// <param name="name">Specifies the file to back up to, the absense of which will result in a default location.</param>
    public void Backup(string? name);

    public void Export(string exportType, string? destination);

    /// <summary>
    /// Gets a string to show the user prior to a diary entry.
    /// </summary>
    /// <returns></returns>
    public string GetPrelogAdvice();

    /// <summary>
    /// Convenience function to convert py data to net environment.
    /// </summary>
    /// <returns></returns>
    public void MigrateDataToNet(string? filePath);

    /// <summary>
    /// Handles Exceptions from Diary.
    /// </summary>
    /// <param name="ex">Exception to Handle</param>
    public void HandleError(Exception ex);
}