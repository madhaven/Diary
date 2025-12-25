using System.ComponentModel.DataAnnotations;

namespace Diary.Web.Contracts;

public record Entry : IValidatableObject
{
    public Guid? Id { get; init; } = Guid.NewGuid();
    public string Text { get; init; } = string.Empty;
    public List<double> Intervals { get; init; } = [];
    public DateTime Time { get; init; } = DateTime.Now;
    public bool PrintDate { get; init; } = false;

    public static Entry FromEntity(Diary.Models.Entry entity)
    {
        return new Entry
        {
            Text = entity.Text,
            Intervals = entity.Intervals,
            Time = entity.Time,
            PrintDate = entity.PrintDate,
        };
    }

    public Diary.Models.Entry ToEntity()
    {
        return new Diary.Models.Entry
        {
            Text = Text,
            Intervals = Intervals,
            Time = Time,
            PrintDate = PrintDate
        };
    }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Text.Length != Intervals.Count)
        {
            yield return new ValidationResult(
                "The number of characters in the text must be equal to the number of intervals.",
                new[] { nameof(Text), nameof(Intervals) });
        }
    }
}