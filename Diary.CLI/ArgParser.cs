using System.CommandLine;

namespace Diary.CLI;

public class ArgParser : IArgParser
{
    private RootCommand _rootCommand;

    public ArgParser(ICliController controller)
    {
        _rootCommand = BuildParser(controller);
    }
    
    public void Obey(string[] args)
    {
        _rootCommand.Parse(args).Invoke();
    }
    
    public RootCommand BuildParser(ICliController controller)
    {
        _rootCommand = new RootCommand("Diary CLI");
        AddLogCommand(controller);
        AddReadCommand(controller);
        AddSearchCommand(controller);
        AddBackup(controller);
        return _rootCommand;
    }

    private void AddLogCommand(ICliController controller)
    {
        var commandLog = new Command("log", "adds entries to the diary");
        commandLog.Aliases.Add("entry");
        commandLog.SetAction(_ => { controller.Log(); });
        _rootCommand.Add(commandLog);
    }

    private void AddReadCommand(ICliController controller)
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
        _rootCommand.Add(commandRead);
    }

    private void AddSearchCommand(ICliController controller)
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
        _rootCommand.Add(commandSearch);
    }

    private void AddBackup(ICliController controller)
    {
        var commandBackup = new Command("backup", "creates a backup of the diary");
        var argumentFilename = new Argument<string>("filename");
        commandBackup.SetAction(parseResult =>
        {
            var fileName = parseResult.GetValue(argumentFilename);
            controller.Backup(fileName);
        });
        commandBackup.Add(argumentFilename);
        _rootCommand.Add(commandBackup);
    }
}