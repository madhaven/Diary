using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Diary.Core;
using Diary.Data;
using Diary.Implementation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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

        using var scope = host.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DiaryDbContext>();
        ctx.Database.EnsureCreated();

        var parser = scope.ServiceProvider.GetRequiredService<IArgParser>();
        parser.Obey(args);
    }

    private static HostApplicationBuilder ConfigureDiaryServices(this HostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICliController, CliController>();
        builder.Services.AddScoped<IDiaryService, DiaryService>();
        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<IArgParser, ArgParser>();

        builder.Services.Configure<AppConfigs>(builder.Configuration.GetSection(nameof(AppConfigs)));

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
        const string fileName = "appsettings.json";
        if (File.Exists(fileName)) return;
        
        var json = JsonSerializer.Serialize(new { AppConfigs = new AppConfigs() });
        File.WriteAllText(fileName, json);
    }
}