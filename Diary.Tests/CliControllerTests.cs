using Diary.CLI;
using Diary.Core;
using Diary.Core.Exceptions;
using Diary.Models;
using Microsoft.Extensions.Options;
using Moq;

namespace Diary.Tests;

public class CliControllerTests
{
    private Mock<IDiaryService> _diaryServiceMock;
    private Mock<IOptions<AppConfigs>> _appConfigMock;
    private Mock<IConsoleWrapper> _consoleWrapperMock;

    [SetUp]
    public void Setup()
    {
        _appConfigMock = new Mock<IOptions<AppConfigs>>();
        _diaryServiceMock = new Mock<IDiaryService>();
        _consoleWrapperMock = new Mock<IConsoleWrapper>();
    }

    [Test]
    public void TestInitThrows()
    {
        Assert.Throws<ArgumentNullException>(() => { _ = new CliController(null!, _appConfigMock.Object, _consoleWrapperMock.Object); });
        Assert.Throws<ArgumentNullException>(() => { _ = new CliController(_diaryServiceMock.Object, null!, _consoleWrapperMock.Object); });
        Assert.Throws<ArgumentNullException>(() => { _ = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, null!); });
    }

    [Test]
    public void TestLogShowsPrelogAdvice()
    {
        var inputQueue = CreateDummyInput("test123\ntest bye\n");
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs());
        _consoleWrapperMock.Setup(x => x.ReadKey(It.IsAny<bool>())).Returns(inputQueue.Dequeue);

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.Log();
        
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Once);
        _diaryServiceMock.Verify(x => x.AddEntry(It.IsAny<Entry>()), Times.Exactly(2));
    }

    [Test]
    public void TestRecord()
    {
        var inputQueue = CreateDummyInput("test bye\n");
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });
        _consoleWrapperMock.Setup(x => x.ReadKey(It.IsAny<bool>())).Returns(inputQueue.Dequeue);

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        var entry = cliController.Record();
        
        Assert.That(entry.ToString(), Is.EqualTo("test bye\n"));
    }


    [Test]
    public void TestReplayEntry()
    {
        var e1 = new Entry { PrintDate = true, Intervals = [1f, 1f, 1f], Text = "yo\n", Time = DateTime.Now };
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.ReplayEntry(e1);

        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Once);
        _consoleWrapperMock.Verify(x => x.Write(It.IsAny<char>()), Times.Exactly(e1.ToString().Length));
    }

    [Test]
    public void TestReplayEntries()
    {
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });
        List<Entry> entries = [
            new() { PrintDate = true, Intervals = [1f, 1f, 1f], Text = "yo\n" },
            new() { PrintDate = false, Intervals = [2f, 2f, 1f], Text = "yo\n", Time = DateTime.Now },
        ];
        
        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.ReplayEntries([]);
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Once);
        
        cliController.ReplayEntries(entries);
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.AtLeast(2));
    }

    [Test]
    public void TestReplayAll()
    {
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.ReplayAll();
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Once);
        _diaryServiceMock.Verify(x => x.All(), Times.Once);
    }

    [Test]
    public void TestReplayToday()
    {
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });
        var now = DateTime.Now;

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.ReplayToday();
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Once);
        _diaryServiceMock.Verify(x => x.Filter(now.Year, now.Month, now.Day), Times.Once);
    }
    
    [Test]
    public void TestReplayYesterday()
    {
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });
        var yesterday = DateTime.Now - TimeSpan.FromDays(1);

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.ReplayYesterday();
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Once);
        _diaryServiceMock.Verify(x => x.Filter(yesterday.Year, yesterday.Month, yesterday.Day), Times.Once);
    }

    [Test]
    public void TestReplayLast()
    {
        var lastEntry = new Entry { PrintDate = true, Intervals = [1f, 1f, 1f], Text = "yo\n", Time = DateTime.Now };
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });
        _diaryServiceMock.Setup(x => x.LastEntry()).Returns(lastEntry);

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.ReplayLast();
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Once);
        _diaryServiceMock.Verify(x => x.Filter(lastEntry.Time.Year, lastEntry.Time.Month, lastEntry.Time.Day));
    }

    [TestCaseSource(nameof(ReplayFromTestData))]
    public void TestReplayFrom(string?[] dates, int? year, int? month, int? day)
    {
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.ReplayFrom(dates);
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.AtLeastOnce);
        _diaryServiceMock.Verify(x => x.Filter(year, month, day), Times.Once);
    }

    [Test]
    public void TestReplayFromThrows()
    {
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });

        var cliController =
            new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.ReplayFrom(["random", "stuff"]);
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void TestReplaySearch()
    {
        string[] keywords = ["test", "test2"];
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.Search(keywords);
        _diaryServiceMock.Verify(x => x.Search(false, keywords), Times.Once);
        cliController.Search(keywords, true);
        _diaryServiceMock.Verify(x => x.Search(true, keywords), Times.Once);
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Exactly(2));
    }

    [Test]
    public void TestReplayBackup()
    {
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.Backup(null!);
        cliController.Backup("test");
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Exactly(4));
        _diaryServiceMock.Verify(x => x.Backup(null), Times.Once);
        _diaryServiceMock.Verify(x => x.Backup("test"), Times.Once);
    }

    [Test]
    public void TestReplayExport()
    {
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        cliController.Export("txt", null);
        cliController.Export("txt", "test");
        _consoleWrapperMock.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Exactly(2));
        _diaryServiceMock.Verify(x => x.Export(ExportOption.Text, It.IsAny<string>()), Times.Exactly(2));
    }

    [Test]
    public void TestGetPrelogAdvice()
    {
        var stopWord = "bye";
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = stopWord });
        var expectedAdvice = $"Ctrl+C or '{stopWord}' to stop recording.\nSay something memorable about today :)\n";

        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        var actualAdvice = cliController.GetPrelogAdvice();
        Assert.That(expectedAdvice, Is.EqualTo(actualAdvice));
    }

    [Test]
    public void TestHandleError()
    {
        _appConfigMock.Setup(x => x.Value).Returns(new AppConfigs { StopWord = "bye" });
        var cliController = new CliController(_diaryServiceMock.Object, _appConfigMock.Object, _consoleWrapperMock.Object);
        
        cliController.HandleError(new InvalidExportException());
        _consoleWrapperMock.Verify(x => x.WriteLine("Invalid export type."), Times.Once);
        cliController.HandleError(new BadFileHeaderException());
        _consoleWrapperMock.Verify(x => x.WriteLine("Bad file header found."), Times.Once);
    }

    private static Queue<ConsoleKeyInfo> CreateDummyInput(string text)
    {
        var keyQueue = new Queue<ConsoleKeyInfo>();
        foreach (var c in text)
        {
            var key = c switch
            {
                '\n' => ConsoleKey.Enter,
                ' ' => ConsoleKey.Spacebar,
                _ => Enum.Parse<ConsoleKey>(c.ToString().ToUpper())
            };
            keyQueue.Enqueue(new ConsoleKeyInfo(c, key, false, false, false));
        }
        return keyQueue;
    }

    private static IEnumerable<TestCaseData> ReplayFromTestData()
    {
        List<TestCaseData<string?[], int?, int?, int?>> testCases = [
            new (["jan"], null, 1, null),
            new (["february"], null, 2, null),
            new (["2025"], 2025, null, null),
            new (["25"], null, null, 25),
            new (["25", "jan"], null, 1, 25),
            new (["jan", "25"], null, 1, 25),
            new (["jan", "2025"], 2025, 1, null),
            new (["2025", "jan"], 2025, 1, null),
            new (["2025", "25"], 2025, null, 25),
            new (["25", "2025"], 2025, null, 25),
            new (["2025", "march", "15"], 2025, 3, 15)
        ];

        foreach (var testCase in testCases)
        {
            yield return testCase;
        }
    }
}