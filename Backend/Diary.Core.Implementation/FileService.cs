using Diary.Core;
using Microsoft.Extensions.Options;

namespace Diary.Implementation;

public class FileService : IFileService
{
    private string FileName { get; }

    public FileService(IOptions<AppConfigs> appconfigs)
    {
        ArgumentNullException.ThrowIfNull(appconfigs);
        var appConfigs = appconfigs.Value;
        FileName = appConfigs.SqlitePath;
        var directory = Path.GetDirectoryName(FileName)!;
        
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        if (!File.Exists(FileName))
            File.Create(FileName).Dispose();
    }

    public void Backup(string? opName)
    {
        if (string.IsNullOrEmpty(opName))
        {
            var directory = Path.GetDirectoryName(FileName);
            var dateString = DateTime.Now.ToString("yyyyMMddHHmm");
            opName = Path.Join(directory, "diarybackup_" + dateString + ".sqlite");
        }
        
        File.Copy(FileName, opName, false);
    }
}