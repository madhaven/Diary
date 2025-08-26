namespace Diary.Core;

public interface IFileService
{
    public string FileName { get; }
    public void Write(params Entry[] entries);
    public IEnumerable<Entry> Load();
    public void Backup(string? opName);
}