namespace Diary.Core;

public interface IDiaryService
{
    /// <summary>
    /// writes the entry/entries to the file
    /// </summary>
    /// <param name="entries"></param>
    public void AddEntry(params Entry[] entries);

    /// <summary>
    /// Scans the file specified on creation and initializes the list of Entry objects
    /// This replaces any existing Entries in memory with data from the file
    ///
    /// Only recommended when reading entries as adding entries to the diary do not require any data in memory  
    /// </summary>
    /// <returns>list of entries</returns>
    public IEnumerable<Entry> FetchEntries();

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
}