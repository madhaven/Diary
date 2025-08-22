using System.Globalization;

namespace Diary.Core;

public class FileManager : IFileManager
{
    public string FileName { get; }

    public FileManager(string fileName)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
    }
    
    public void Write(params Entry[] entries)
    {
        using var stream = new FileStream(FileName, FileMode.Append, FileAccess.Write);
        using var writer = new StreamWriter(stream);
        foreach (var entry in entries)
        {
            writer.WriteLine(entry.Time.ToString("yyyy-MM-dd HH:mm:ss"));
            writer.Write(entry.Text); // \n will exist at end of text line
            writer.WriteLine(string.Join(',', entry.Intervals.Select(x => x.ToString(CultureInfo.InvariantCulture))));
            writer.WriteLine();
        }
    }

    public IEnumerable<Entry> Load()
    {
        var entries = new List<Entry>();
        string? line;
        using var stream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Read);
        using var reader = new StreamReader(stream);
        while ((line = reader.ReadLine()) != null)
        {
            var datetime = DateTime.ParseExact(line, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            var text = reader.ReadLine() ?? throw new Exception("Corrupt Entry"); // TODO: move to seperate exception
            text += '\n';
            var intervals = reader.ReadLine()
                ?.Split(',').Select(int.Parse).ToList() ?? throw new Exception("Corrupt Entry");
            var entry = new Entry(text, datetime, intervals);
            entries.Add(entry);
            reader.ReadLine();
        }
        
        return entries;
    }

    public void Backup(string name)
    {
        ;
    }
}