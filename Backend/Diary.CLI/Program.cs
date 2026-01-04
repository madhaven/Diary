using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Diary.Data;
using Diary.Implementation;
using Microsoft.EntityFrameworkCore;

namespace Diary.CLI;

internal static class Program
{
    private static void Main(string[] args)
    {
        // base dir setup // TODO: can we improve this logic using dotnet builtins?
        var executableDir = Path.GetDirectoryName(AppContext.BaseDirectory)!;
        Utils.EnsureConfigExists(executableDir);

        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = executableDir,
            Args = args
        })
        .ConfigureDiaryServices(); // Dependency Injection / Inversion of Control

        var host = builder.Build();

        // DB setup
        using var scope = host.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DiaryDbContext>();
        ctx.Database.Migrate();

        // parse args
        Console.CancelKeyPress += (_, _) => { Console.WriteLine("\nDiary closed"); }; // TODO: add tests
        var parser = scope.ServiceProvider.GetRequiredService<IArgParser>();
        parser.ParseAndInvoke(args);
    }

}