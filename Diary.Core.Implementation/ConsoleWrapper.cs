using Diary.Core;

namespace Diary.Implementation;

public class ConsoleWrapper : IConsoleWrapper
{
    public bool KeyAvailable => Console.KeyAvailable;

    public void WriteLine()
    {
        Console.WriteLine();
    }

    public void WriteLine(string? value)
    {
        Console.WriteLine(value);
    }

    public void Write(object? value)
    {
        Console.Write(value);
    }

    public ConsoleKeyInfo ReadKey(bool intercept)
    {
        return Console.ReadKey(intercept);
    }
}