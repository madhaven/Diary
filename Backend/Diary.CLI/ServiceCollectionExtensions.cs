// Keep DI in this file in sync with Diary.Web project.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Diary.Core;
using Diary.Data;
using Diary.Implementation;
using Diary.Implementation.ExportStrategies;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Diary.CLI;

public static class ServiceCollectionExtensions
{
    public static HostApplicationBuilder ConfigureDiaryServices(this HostApplicationBuilder builder)
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
}
