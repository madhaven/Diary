using Diary.Core;
using Diary.Data;
using Diary.Implementation;
using Diary.Implementation.ExportStrategies;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Diary.Web;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ConfigureDiaryServices(this WebApplicationBuilder builder)
    {
        // DI service configuration
        builder.Services.AddScoped<IConsoleWrapper, ConsoleWrapper>();
        builder.Services.AddScoped<IDiaryService, DiaryService>();
        builder.Services.AddScoped<IFileService, FileService>();

        // Exporter classes configuration
        builder.Services.AddScoped<IExporterFactory, ExporterFactory>();
        builder.Services.AddTransient<TextExportStrategy>();

        // App Configs
        builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(nameof(AppConfigs)));

        // DB setup
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = builder.Configuration["AppConfigs:SqlitePath"] ?? Path.Combine(AppContext.BaseDirectory, "diary.sqlite"), // Use BaseDirectory for web app
            Cache = SqliteCacheMode.Shared,
            Mode = SqliteOpenMode.ReadWriteCreate
        };
        var connectionString = connectionStringBuilder.ToString();
        builder.Services.AddDbContext<DiaryDbContext>(options => options.UseSqlite(connectionString));
        
        // Controllers and swagger
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        return builder;
    }

}