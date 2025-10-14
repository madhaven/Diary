using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Diary.Core;
using Diary.Data;
using Diary.Implementation;
using Diary.Implementation.ExportStrategies;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Diary.CLI;

internal static class Program
{
    private const string AppSettingsFile = "appsettings.json";

    private static void Main(string[] args)
    {
        EnsureConfigExists();

        var builder = Host.CreateApplicationBuilder(args)
            .ConfigureDiaryServices();

        // TODO: add tests
        Console.CancelKeyPress += (_, _) => { Console.WriteLine("\nDiary closed"); };

        var host = builder.Build();

        using var scope = host.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DiaryDbContext>();
        ctx.Database.Migrate();

        var parser = scope.ServiceProvider.GetRequiredService<IArgParser>();
        parser.ParseAndInvoke(args);
    }

    private static HostApplicationBuilder ConfigureDiaryServices(this HostApplicationBuilder builder)
    {
        // DI service configuration
        builder.Services.AddScoped<IConsoleWrapper, ConsoleWrapper>();
        builder.Services.AddScoped<ICliController, CliController>();
        builder.Services.AddScoped<IDiaryService, DiaryService>();
        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<IArgParser, ArgParser>();

        // Exporter classes configuration
        builder.Services.AddScoped<IExportStrategyFactory, ExportStrategyFactory>();
        builder.Services.AddTransient<TextExportStrategy>();

        // App Configs
        builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(nameof(AppConfigs)));

        // DB setup
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = builder.Configuration["AppConfigs:SqlitePath"],
            Cache = SqliteCacheMode.Shared,
            Mode = SqliteOpenMode.ReadWriteCreate
        };
        var connectionString = connectionStringBuilder.ToString();
        builder.Services.AddDbContext<DiaryDbContext>(options => options.UseSqlite(connectionString));

        return builder;
    }

    private static void EnsureConfigExists()
    {
        if (File.Exists(AppSettingsFile)) return;

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
        File.WriteAllText(AppSettingsFile, json);
    }
}
