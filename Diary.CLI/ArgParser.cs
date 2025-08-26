using System.CommandLine;

namespace Diary.CLI;

public static class ArgParser
{
    /// <summary>
    /// Takes in the arguments, Parses them and fires actions with the controller.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="controller"></param>
    public static void Obey(string[] args, ICliController controller)
    {
        var parser = ArgParser.BuildParser(controller);
        parser.Parse(args).Invoke();
    }
    
    /// <summary>
    /// Builds the parser which can be used to parse arguments and thereby invoke actions.
    /// </summary>
    /// <param name="controller"></param>
    /// <returns>RootCommand</returns>
    public static RootCommand BuildParser(ICliController controller)
    {
        return new RootCommand("Diary CLI")
            .AddLogCommand(controller)
            .AddReadCommand(controller)
            .AddSearchCommand(controller)
            .AddBackup(controller);
    }

    private static RootCommand AddLogCommand(this RootCommand rootCommand, ICliController controller)
    {
        var commandLog = new Command("log", "adds entries to the diary");
        commandLog.Aliases.Add("entry");
        commandLog.SetAction(_ => { controller.Log(); });
        
        rootCommand.Add(commandLog);
        return rootCommand;
    }

    private static RootCommand AddReadCommand(this RootCommand rootCommand, ICliController controller)
    {
        var commandRead = new Command("read", "replay previously created entries");
        commandRead.Aliases.Add("show");
        var commandFrom = new Command("from", "read entries from a specific year/month/date");
        var filter1 = new Argument<string>("y/m/d");
        commandFrom.Add(filter1);
        var filter2 = new Argument<string?>("y/m/d") { DefaultValueFactory = _ => null};
        commandFrom.Add(filter2);
        var filter3 = new Argument<string?>("y/m/d") { DefaultValueFactory = _ => null};
        commandFrom.Add(filter3);
        commandFrom.SetAction(parseResult =>
        {
            List<Argument<string?>> dateSpec = [filter1!, filter2, filter3];
            var dates = dateSpec.Select(parseResult.GetValue);
            controller.ReplayFrom(dates.ToList());
        });
        commandRead.Add(commandFrom);
        var commandReadAll = new Command("all", "read all entries from the very start");
        commandReadAll.SetAction(_ => { controller.ReplayAll(); });
        commandRead.Add(commandReadAll);
        var commandReadToday =  new Command("today", "read entries from current day");
        commandReadToday.SetAction(_ => { controller.ReplayToday(); });
        commandRead.Add(commandReadToday);
        var commandReadYesterday = new Command("yesterday", "read entries from yesterday");
        commandReadYesterday.SetAction(_ => { controller.ReplayYesterday(); });
        commandRead.Add(commandReadYesterday);
        
        rootCommand.Add(commandRead);
        return rootCommand;
    }

    private static RootCommand AddSearchCommand(this RootCommand rootCommand, ICliController controller)
    {
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

        rootCommand.Add(commandSearch);
        return rootCommand;
    }

    private static RootCommand AddBackup(this RootCommand rootCommand, ICliController controller)
    {
        var commandBackup = new Command("backup", "creates a backup of the diary");
        var argumentFilename = new Argument<string>("filename");
        commandBackup.SetAction(parseResult =>
        {
            var fileName = parseResult.GetValue(argumentFilename);
            controller.Backup(fileName);
        });
        commandBackup.Add(argumentFilename);

        rootCommand.Add(commandBackup);
        return rootCommand;
    }
}