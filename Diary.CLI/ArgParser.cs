using System.CommandLine;

namespace Diary.CLI;

public class ArgParser : IArgParser
{
    private readonly RootCommand _rootCommand;
    private readonly ICliController _controller;

    public ArgParser(ICliController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _rootCommand = new RootCommand("Diary CLI");
        AddLogCommand();
        AddReadCommand();
        AddSearchCommand();
        AddBackup();
        AddExport();
        AddHiddenMigrate();
    }
    
    public void ParseAndInvoke(string[] args)
    {
        try
        {
            _rootCommand.Parse(args).Invoke();
        }
        catch (Exception ex)
        {
            _controller.HandleError(ex);
        }
    }

    private void AddLogCommand()
    {
        var commandLog = new Command("log", "adds entries to the diary");
        commandLog.Aliases.Add("entry");
        commandLog.SetAction(_ => { _controller.Log(); });
        _rootCommand.Add(commandLog);
    }

    private void AddReadCommand()
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
            _controller.ReplayFrom(dates.ToArray());
        });
        commandRead.Add(commandFrom);
        var commandReadAll = new Command("all", "read all entries from the very start");
        commandReadAll.SetAction(_ => { _controller.ReplayAll(); });
        commandRead.Add(commandReadAll);
        var commandReadToday =  new Command("today", "read entries from current day");
        commandReadToday.SetAction(_ => { _controller.ReplayToday(); });
        commandRead.Add(commandReadToday);
        var commandReadYesterday = new Command("yesterday", "read entries from yesterday");
        commandReadYesterday.SetAction(_ => { _controller.ReplayYesterday(); });
        commandRead.Add(commandReadYesterday);
        var commandLast = new Command("last", "read entries from the last day entered");
        commandLast.SetAction(_ => { _controller.ReplayLast(); });
        commandRead.Add(commandLast);
        _rootCommand.Add(commandRead);
    }

    private void AddSearchCommand()
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
            _controller.Search(keywords, isStrict);
        });
        commandSearch.Add(optionStrict);
        _rootCommand.Add(commandSearch);
    }

    private void AddBackup()
    {
        var commandBackup = new Command("backup", "creates a backup of the diary");
        var argumentFilename = new Argument<string?>("filename")
        {
            DefaultValueFactory = _ => null
        };
        commandBackup.SetAction(parseResult =>
        {
            var fileName = parseResult.GetValue(argumentFilename);
            _controller.Backup(fileName);
        });
        commandBackup.Add(argumentFilename);
        _rootCommand.Add(commandBackup);
    }

    private void AddExport()
    {
        var commandExport = new Command("export", "exports diary data to specified format");
        var argumentFileType = new Argument<string>("fileformat")
            .AcceptOnlyFromAmong("txt", "csv");
        var argumentDestination = new Argument<string?>("destination")
        {
            Description = "Filename without the extension",
            DefaultValueFactory = _ => null
        };
        commandExport.SetAction(result =>
        {
            var exportType = result.GetValue(argumentFileType)!.ToLower();
            var fileDestination = result.GetValue(argumentDestination);
            _controller.Export(exportType, fileDestination);
        });
        commandExport.Add(argumentFileType);
        commandExport.Add(argumentDestination);
        _rootCommand.Add(commandExport);
    }

    /// <summary>
    /// This is a hidden migrate option for converting python data files to dotnet implementation.
    /// This is not for the general user.
    /// </summary>
    private void AddHiddenMigrate()
    {
        var commandMig = new Command("mig2.net")
        {
            Hidden = true,
            Description = "converts previous python data file into dotnet implementation",
        };
        var argumentFileLoc = new Argument<string?>("fileloc") { DefaultValueFactory = _ => null };
        commandMig.SetAction(result =>
        {
            var loc = result.GetValue(argumentFileLoc);
            _controller.MigrateDataToNet(loc);
        });
        commandMig.Add(argumentFileLoc);
        _rootCommand.Add(commandMig);
    }
}