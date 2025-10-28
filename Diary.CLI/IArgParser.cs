namespace Diary.CLI;

public interface IArgParser
{
    /// <summary>
    /// Takes in the arguments, Parses them and fires actions with the controller.
    /// </summary>
    /// <param name="args"></param>
    public void ParseAndInvoke(string[] args);
}