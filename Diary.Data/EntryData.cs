using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Diary.Models;

namespace Diary.Data;

[Table("Entries")]
public class EntryData
{
    [Key]
    public int Id { get; set; }
    [Required]
    public DateTime Time { get; set; }
    public string? Text { get; set; }
    public string? Intervals { get; set; }

    public static EntryData FromEntity(Entry entry)
    {
        return new EntryData
        {
            Intervals = string.Join(",", entry.Intervals),
            Time = entry.Time,
            Text = entry.Text
        };
    }

    public Entry ToEntity()
    {
        return new Entry
        {
            Intervals = Intervals?.Split(",").Select(int.Parse).ToList() ?? throw new DataException("Data corrupt"),
            Time = Time,
            Text = Text ?? throw new DataException("Data corrupt")
        };
    }
}
