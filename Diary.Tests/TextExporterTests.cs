using Diary.Core;
using Diary.Implementation.Export;
using Diary.Models;

namespace Diary.Tests;

public class TextExporterTests
{
    private IExportStrategy _strategy;
    private const string Destination = ".\\test.txt";
    private const string NonExistantDestination = ".\\newtestfolder\\test.txt";
    private const string NonExistantFolder = ".\\newtestfolder";
    private const string FileWithoutExtension = ".\\test";

    [SetUp]
    public void Setup()
    {
        _strategy = new TextExporter(); 
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(Destination))
            File.Delete(Destination);
        if (File.Exists(NonExistantDestination))
            File.Delete(NonExistantDestination);
        if (Directory.Exists(NonExistantFolder))
            Directory.Delete(NonExistantFolder, true);
        if (File.Exists(FileWithoutExtension))
            File.Delete(FileWithoutExtension);
    }

    [Test]
    public void Export_EmptyEntriesList_DoesNothing()
    {
        var entries = new List<Entry>();
        _strategy.Export(entries, Destination);
        Assert.That(File.Exists(Destination), Is.False, "File should not be created for empty entries.");
    }

    [Test]
    public void Export_SingleEntry_CreatesCorrectFile()
    {
        List<Entry> entries = [
            new()
            {
                Time = new DateTime(2023, 11, 15, 11, 0, 0, DateTimeKind.Local),
                Text = "A single entry test."
            }
        ];
        var dateTime = new DateTime(2023, 11, 15, 11, 0, 0, DateTimeKind.Local);
        var expectedContent = $"{dateTime:ddd yyyy-MMM-dd HH:mm:ss}\nA single entry test.";

        _strategy.Export(entries, Destination);
        AssertFileContent(Destination, expectedContent);
    }

    [Test]
    public void Export_MultipleEntriesDifferentDates_CreatesCorrectFile()
    {
        var entries = CreateMockEntries();
        var expectedContent = 
            $"{new DateTime(2023, 10, 26, 10, 0, 0, DateTimeKind.Local):ddd yyyy-MMM-dd HH:mm:ss}\nFirst entry for the day.\n" +
            $"Second entry, same day.\n" +
            $"\n{new DateTime(2023, 10, 27, 9, 15, 0, DateTimeKind.Local):ddd yyyy-MMM-dd HH:mm:ss}\nEntry for the next day.\n";

        _strategy.Export(entries, Destination);
        AssertFileContent(Destination, expectedContent);
    }

    [Test]
    public void Export_MultipleEntriesSameDate_CreatesCorrectFile()
    {
        var entries = new List<Entry>
        {
            new() { Time = new DateTime(2023, 12, 1, 10, 0, 0, DateTimeKind.Local), Text = "First entry." },
            new() { Time = new DateTime(2023, 12, 1, 11, 0, 0, DateTimeKind.Local), Text = "Second entry." }
        };
        var expectedContent = 
            $"{new DateTime(2023, 12, 1, 10, 0, 0, DateTimeKind.Local):ddd yyyy-MMM-dd HH:mm:ss}\nFirst entry." +
            $"Second entry.";

        _strategy.Export(entries, Destination);
        AssertFileContent(Destination, expectedContent);
    }

    [Test]
    public void Export_DestinationDirectoryDoesNotExist_CreatesDirectoryAndFile()
    {
        var entries = new List<Entry> { new() { Time = DateTime.Now, Text = "Test content.\n" } };
        var destination = NonExistantDestination; // Directory does not exist
        var expectedContent = $"{DateTime.Now:ddd yyyy-MMM-dd HH:mm:ss}\nTest content.\n";

        _strategy.Export(entries, destination);
        Assert.That(Directory.Exists(NonExistantFolder), Is.True, "The destination directory should be created.");
        AssertFileContent(destination, expectedContent);
    }

    [Test]
    public void Export_DestinationMissingExtension_AppendsExtension()
    {
        var entries = new List<Entry> { new() { Time = DateTime.Now, Text = "Test content.\n" } };
        var destination = FileWithoutExtension;
        var expectedDestination = FileWithoutExtension + ".txt";
        var expectedContent = $"{DateTime.Now:ddd yyyy-MMM-dd HH:mm:ss}\nTest content.\n";

        _strategy.Export(entries, destination);
        AssertFileContent(expectedDestination, expectedContent);
    }
    
    private List<Entry> CreateMockEntries()
    {
        return
        [
            new() { Time = new(2023, 10, 26, 10, 0, 0, DateTimeKind.Local), Text = "First entry for the day.\n" },
            new() { Time = new(2023, 10, 26, 14, 30, 0, DateTimeKind.Local), Text = "Second entry, same day.\n" },
            new() { Time = new(2023, 10, 27, 9, 15, 0, DateTimeKind.Local), Text = "Entry for the next day.\n" }
        ];
    }

    private static void AssertFileContent(string filePath, string expectedContent)
    {
        Assert.That(File.Exists(filePath), Is.True, $"File '{filePath}' should exist.");
        var actualContent = File.ReadAllText(filePath);
        Assert.That(expectedContent, Is.EqualTo(actualContent));
    }
}