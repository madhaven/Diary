using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Diary.Core;

namespace Diary.CLI;

internal static class Program
{
    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args)
            .AddDiaryConfiguration()
            .ConfigureDiaryServices();

        var host = builder.Build();

        var parser = host.Services.GetService<IArgParser>()!;
        parser.Obey(args);

        Console.CancelKeyPress += (_, _) => { Console.WriteLine("\nDiary closed"); };
    }
    
    private static HostApplicationBuilder AddDiaryConfiguration(this HostApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);
        return builder;
    }

    private static HostApplicationBuilder ConfigureDiaryServices(this HostApplicationBuilder builder)
    {
        builder.Services.AddTransient<ICliController, CliController>();
        builder.Services.AddSingleton<IDiaryService, DiaryService>();
        builder.Services.AddSingleton<IFileService, FileService>();
        builder.Services.AddSingleton<IArgParser, ArgParser>();
        return builder;
    }
}