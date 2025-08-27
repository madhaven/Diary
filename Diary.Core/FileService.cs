using System.Globalization;
using Diary.Core.Exceptions;
using Microsoft.Extensions.Options;

namespace Diary.Core;

public class FileService : IFileService
{
    private string FileName { get; }

    public FileService(IOptions<AppConfigs> appconfigs)
    {
        var appConfigs = appconfigs.Value ?? throw new ArgumentNullException(nameof(appconfigs));
        FileName = appConfigs.FileLocation;
        var directory = Path.GetDirectoryName(FileName)!;
        
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        if (!File.Exists(FileName))
            File.Create(FileName).Dispose();
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
            var printDate = entries.LastOrDefault()?.Time.Date != datetime.Date;
            var text = reader.ReadLine() ?? throw new BadFileHeaderException();
            text += '\n';
            var intervals = reader.ReadLine()
                ?.Split(',').Select(int.Parse) ?? throw new BadFileHeaderException();
            var entry = new Entry(text, datetime, intervals, printDate);
            entries.Add(entry);
            reader.ReadLine();
        }
        
        return entries;
    }

    public void Backup(string? opName)
    {
        if (string.IsNullOrEmpty(opName))
        {
            var directory = Path.GetDirectoryName(FileName);
            var dateString = DateTime.Now.ToString("yyyyMMddHHmm");
            opName = Path.Join(directory, "diaryback_" + dateString);
        }
        
        File.Copy(FileName, opName, false);
    }
}