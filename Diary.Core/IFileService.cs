using Diary.Models;

namespace Diary.Core;

public interface IFileService
{
    public void Write(params Entry[] entries);
    public IEnumerable<Entry> Load();
    public void Backup(string? opName);
}