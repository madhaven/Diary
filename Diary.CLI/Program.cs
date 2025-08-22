using System.CommandLine;

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
        var fileManager = new FileManager("diary.txt"); // TODO: fetch from configs
        var diary = new Diary(fileManager);
        var controller = new CliController(diary, "bye", 1);

        var parser = BuildParser(controller);
        parser.Parse(args).Invoke();
    }
    
    private static RootCommand BuildParser(CliController controller)
    {
        // log
        var commandLog = new Command("log", "adds entries to the diary");
        commandLog.Aliases.Add("entry");
        commandLog.SetAction(_ => { controller.Log(); });

        // read
        var commandRead = new Command("read", "replay previously created entries");
        commandRead.Aliases.Add("show");
        var commandFrom = new Command("from", "access entries from a specific year/month/date");
        var filter1 = new Argument<string>("y/m/d");
        commandFrom.Add(filter1);
        var filter2 = new Argument<string?>("y/m/d") { DefaultValueFactory = x => null};
        commandFrom.Add(filter2);
        var filter3 = new Argument<string?>("y/m/d") { DefaultValueFactory = x => null};
        commandFrom.Add(filter3);
        commandFrom.SetAction(parseResult =>
        {
            List<Argument<string?>> dateSpec = [filter1!, filter2, filter3];
            var dates = dateSpec.Select(parseResult.GetValue);
            controller.ReplayFrom(dates.ToList());
        });
        commandRead.Add(commandFrom);
        var commandReadAll = new Command("all", "read all entries from the very start");
        commandReadAll.SetAction(parseResult => { controller.ReplayAll(); });
        commandRead.Add(commandReadAll);
        var commandReadToday =  new Command("today", "read entries from current day");
        commandReadToday.SetAction(parseResult => { controller.ReplayToday(); });
        commandRead.Add(commandReadToday);
        var commandReadYesterday = new Command("yesterday", "read entries from yesterday");
        commandReadYesterday.SetAction(parseResult => { controller.ReplayYesterday(); });
        commandRead.Add(commandReadYesterday);
        
        // search
        var commandSearch = new Command("search", "search for keywords");
        var optionStrict = new Option<bool>("--strict");
        var argumentKeywords = new Argument<string[]>("keywords");
        commandSearch.Aliases.Add("find");
        commandSearch.Add(argumentKeywords);
        commandSearch.SetAction(parseResult =>
        {
            var isStrict = parseResult.GetValue(optionStrict);
            var keywords = parseResult.GetValue(argumentKeywords)!;
            controller.Search(keywords, isStrict);
        });
        commandSearch.Add(optionStrict);

        // backup
        var commandBackup = new Command("backup", "creates a backup of the diary");
        var argumentFilename = new Argument<string>("filename");
        commandBackup.SetAction(parseResult => { Console.WriteLine("backup"); });
        commandBackup.Add(argumentFilename);
        
        // root
        var root = new RootCommand("Diary CLI");
        root.Add(commandLog);
        root.Add(commandRead);
        root.Add(commandSearch);
        root.Add(commandBackup);
        return root;
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