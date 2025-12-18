namespace Diary.Web.Contracts;

public record Entry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Text { get; init; } = string.Empty;
    public IEnumerable<double> Intervals { get; init; } = [];

    public static Entry FromEntity(Diary.Models.Entry entity)
    {
        return new Entry
        {
            Text = entity.Text,
            Intervals = entity.Intervals,
        };
    }
}