namespace Diary.Core;

public class AppConfigs
{
    public float ReplaySpeed { get; set; } = 1.5f;
    public string StopWord { get; set; } = "bye";
    public string SqlitePath { get; set; }

    public AppConfigs()
    {
        var userPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        SqlitePath = Path.Combine(userPath, "Diary", "diary.sqlite");
    }
}