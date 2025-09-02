namespace Diary.Models;

public class Entry
{
    public string Text { get; set; }
    public DateTime Time { get; init; }
    public bool PrintDate { get; }
    public List<int> Intervals { get; init; }

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
        if (obj is not Entry otherEntry) return false;
        if (ReferenceEquals(this, obj)) return true;

        return Text == otherEntry.Text
            && Time == otherEntry.Time
            && PrintDate == otherEntry.PrintDate
            && Intervals.Count == otherEntry.Intervals.Count
            && Intervals.SequenceEqual(otherEntry.Intervals);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Text);
        hash.Add(Time);
        hash.Add(PrintDate);
        foreach (var interval in Intervals) { hash.Add(interval); }
        return hash.ToHashCode();
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
