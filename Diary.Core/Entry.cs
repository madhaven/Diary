namespace Diary.Core;

public class Entry
{
    public string Text { get; private set; }
    public DateTime Time { get; init; }
    public bool PrintDate { get; }
    public List<int> Intervals { get; }

    public Entry(string text = "", DateTime time = default, IEnumerable<int>? intervals = null, bool printDate = false)
    {
        Text = text;
        Time = time;
        PrintDate = printDate;

        var i2 = intervals?.ToList() ?? Enumerable.Repeat(0, Text.Length).ToList();
        Intervals = i2.Count == Text.Length
            ? i2
            : Enumerable.Repeat(0, Text.Length).ToList();
    }
    
    public bool IsEmpty() => Text.Length == 0;

    public override bool Equals(object? obj)
    {
        return obj is Entry model && (Text == model.Text || Intervals == model.Intervals);
    }

    /// <summary>
    /// Convert user input information and returns data alone
    /// used in search ops where backspace characters need not be considered
    /// in the entry
    /// </summary>
    /// <returns>string</returns>
    public override string ToString()
    {
        string text = "";
        foreach (var character in Text)
        {
            text = character == '\b' && text.Length > 0
                ? text.Substring(0, text.Length - 1)
                : text + character;
        }

        return text;
    }

    /// <summary>
    /// adds another character to the entry.
    /// the time attribute saves the time taken before/after the keypress.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="interval"></param>
    public void AddCharacter(char character, int interval)
    {
        Text += character;
        Intervals.Add(interval);
    }
}
