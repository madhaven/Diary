using System.Text.Json;
using Diary.Core;

namespace Diary.Implementation;

public static class Utils
{
    private const string AppSettingsFile = "appsettings.json";

    public static void EnsureConfigExists(string executableDir)
    {
        var fullAppSettingsPath = Path.Combine(executableDir, AppSettingsFile);
        if (File.Exists(fullAppSettingsPath)) return;

        var defaultConfigs = new
        {
            Logging = new
            {
                LogLevel = new Dictionary<string, string>
                {
                    { "Default", "Error" },
                    { "Microsoft", "Error" },
                    { "Microsoft.Hosting.Lifetime", "Error" },
                    { "Microsoft.EntityFrameworkCore", "Error" }
                }
            },
            AppConfigs = new AppConfigs()
        };
        var json = JsonSerializer.Serialize(defaultConfigs, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fullAppSettingsPath, json);
    }
}