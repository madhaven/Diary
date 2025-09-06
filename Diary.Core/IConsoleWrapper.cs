namespace Diary.Core;

/// <summary>
/// Wraps Console to aid testing
/// </summary>
public interface IConsoleWrapper
{
    public bool KeyAvailable { get; }
    public void WriteLine();
    public void WriteLine(string? value);
    public void Write(object? value);
    public ConsoleKeyInfo ReadKey(bool intercept);
}