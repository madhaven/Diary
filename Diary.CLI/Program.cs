using System.CommandLine;
using Diary.Core;

namespace Diary.CLI;

internal static class Program
{
    /// <summary>
    /// Command Line Access Point to the App
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        // TODO: setup DI
        var fileManager = new FileService("diary.txt"); // TODO: fetch from configs
        var diary = new Core.Diary(fileManager);
        var controller = new CliController(diary, "bye", 1);

            Console.CancelKeyPress += (_, _) => { Console.WriteLine("\nDiary closed"); };

        ArgParser.Obey(args, controller);
    }

    // private static void LoadConfig()
    // {
    //     var config = new ConfigurationBuilder()
    //         .SetBasePath(Directory.GetCurrentDirectory())
    //         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    //         .Build();
    //     AppConfigs configs = config.GetSection("AppSettings");
    // }
}