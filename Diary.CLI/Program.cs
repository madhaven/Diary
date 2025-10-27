using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Diary.Core;
using Diary.Data;
using Diary.Implementation;
using Diary.Implementation.ExportStrategies;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Diary.CLI;

internal static class Program
{
    private const string AppSettingsFile = "appsettings.json";

    private static void Main(string[] args)
    {
        // base dir setup // TODO: can we improve this logic using dotnet builtins?
        var executableDir = Path.GetDirectoryName(AppContext.BaseDirectory)!;
        EnsureConfigExists(executableDir);

        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = executableDir,
            Args = args
        })
        .ConfigureDiaryServices();

        var host = builder.Build();

        // DB setup
        using var scope = host.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DiaryDbContext>();
        ctx.Database.EnsureCreated();
        if (ctx.Database.HasPendingModelChanges()) { ctx.Database.Migrate(); }

        // parse args
        Console.CancelKeyPress += (_, _) => { Console.WriteLine("\nDiary closed"); }; // TODO: add tests
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
        builder.Services.AddScoped<IExporterFactory, ExporterFactory>();
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

    private static void EnsureConfigExists(string executableDir)
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