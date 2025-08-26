namespace Diary.Core;

public class AppConfigs
{
    public string FileLocation { get; set; } = "diary.txt";
    public float ReplaySpeed { get; set; } = 1.5f;
    public string StopWord { get; set; } = "bye";
}