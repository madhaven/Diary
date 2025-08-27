using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Diary.Core;

namespace Diary.CLI;

internal static class Program
{
    private static void Main(string[] args)
    {
        EnsureConfigExists();

        var builder = Host.CreateApplicationBuilder(args)
            .ConfigureDiaryServices();

        Console.CancelKeyPress += (_, _) => { Console.WriteLine("\nDiary closed"); };

        var host = builder.Build();

        host.Services.GetService<IArgParser>()!.Obey(args);
    }

    private static HostApplicationBuilder ConfigureDiaryServices(this HostApplicationBuilder builder)
    {
        builder.Services.AddTransient<ICliController, CliController>();
        builder.Services.AddSingleton<IDiaryService, DiaryService>();
        builder.Services.AddSingleton<IFileService, FileService>();
        builder.Services.AddSingleton<IArgParser, ArgParser>();

        builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(nameof(AppConfigs)));

        return builder;
    }

    private static void EnsureConfigExists()
    {
        const string fileName = "appsettings.json";
        if (File.Exists(fileName)) return;
        
        var json = JsonSerializer.Serialize(new { AppConfigs = new AppConfigs() });
        File.WriteAllText(fileName, json);
    }
}