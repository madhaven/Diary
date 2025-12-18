namespace Diary.Web.Contracts;

public class SearchRequest
{
    public bool IsStrict { get; init; } = false;
    public string[] Queries { get; init; } = Array.Empty<string>();
}