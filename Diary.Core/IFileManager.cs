namespace Diary.Core;

public interface IFileManager
{
    public string FileName { get; }
    public void Write(params Entry[] entries);
    public IEnumerable<Entry> Load();
    public void Backup(string name);
}