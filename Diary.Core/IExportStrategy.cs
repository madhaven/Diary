using Diary.Models;

namespace Diary.Core;

/// <summary>
/// Class that generates data from diary for export.
/// </summary>
public interface IExportStrategy
{
    public string FileExtension { get; }
    public void Export(List<Entry> entries, string destination);
}