using System.CommandLine;

namespace Diary.CLI;

public interface IArgParser
{
    /// <summary>
    /// Takes in the arguments, Parses them and fires actions with the controller.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="controller"></param>
    public void Obey(string[] args);

    /// <summary>
    /// Builds the parser which can be used to parse arguments and thereby invoke actions.
    /// </summary>
    /// <param name="controller"></param>
    /// <returns>RootCommand</returns>
    public RootCommand BuildParser(ICliController controller);
}