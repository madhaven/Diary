using Diary.Core;
using Diary.Models;

namespace Diary.Implementation.ExportStrategies;

public class TextExportStrategy : IExportStrategy
{
    public string FileExtension => ".txt";

    public void Export(List<Entry> entries, string destination)
    {
        if (entries.Count == 0) return;
        
        // setup destination directory
        var targetDir = Path.GetDirectoryName(destination);
        if (!string.IsNullOrWhiteSpace(targetDir) && !Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);
        if (!destination.EndsWith(FileExtension)) destination += FileExtension;
        
        using var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write);
        using var streamWriter = new StreamWriter(fileStream);
        var lastSeenDate = DateTime.UnixEpoch;
        var isFirstEntry = true;
        foreach (var entry in entries.ToList())
        {
            if (lastSeenDate.Date != entry.Time.Date)
            {
                if (!isFirstEntry)
                    streamWriter.Write("\n");
                streamWriter.Write($"{entry.Time:ddd yyyy-MMM-dd HH:mm:ss}\n");
                isFirstEntry = false;
            }
            
            streamWriter.Write(entry.Text);
            lastSeenDate = entry.Time;
        }
    }
}