using Diary.Models;

namespace Diary.Core;

public interface IDiaryService
{
    /// <summary>
    /// writes the entry/entries to the file
    /// </summary>
    /// <param name="entries"></param>
    public void AddEntry(params Entry[] entries);

    /// <summary>
    /// Returns the last entry made in the diary.
    /// </summary>
    /// <returns>Entry</returns>
    public Entry? LastEntry();

    /// <summary>
    /// fetches diary records acc to date match, if `fetchLatest` is set to True.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <returns>list of entries</returns>
    public IEnumerable<Entry> Filter(int? year, int? month, int? day);

    /// <summary>
    /// fetches all diary records.
    /// </summary>
    /// <returns>list of entries</returns>
    public IEnumerable<Entry> All();

    /// <summary>
    /// Searches for records containing the keywords.
    /// </summary>
    /// <param name="isStrict">filters only entries with all keywords when true</param>
    /// <param name="args">keywords for search</param>
    /// <returns>list of entries</returns>
    public IEnumerable<Entry> Search(bool isStrict = false, params string[] args);

    /// <summary>
    /// Backs up diary.
    /// </summary>
    /// <param name="name">Uses the file if specified</param>
    /// <param name="args">future proofing</param>
    public void Backup(string? name, params object[] args);

    /// <summary>
    /// Exports diary entries using the strategy selected by <paramref name="exportOption"/>.
    /// </summary>
    /// <param name="exportOption">ExportOption that helps resolve the strategy</param>
    /// <param name="destination">Path to save the exported file.</param>
    public void Export(ExportOption exportOption, string destination);
    
    /// <summary>
    /// Convenience function to migrate old python data to net environment
    /// </summary>
    /// <param name="filePath"></param>
    public string MigrateDataToNet(string filePath);
}