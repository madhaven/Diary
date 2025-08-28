namespace Diary.Core;

public class AppConfigs
{
    public string FileLocation { get; set; }
    public float ReplaySpeed { get; set; } = 1.5f;
    public string StopWord { get; set; } = "bye";
    public string SqlitePath { get; set; }

    public AppConfigs()
    {
        var userPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        FileLocation = Path.Combine(userPath, "Diary", "diary");
        SqlitePath = Path.Combine(userPath, "Diary", "diary.sqlite");
    }
}