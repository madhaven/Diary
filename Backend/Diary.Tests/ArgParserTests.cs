using Diary.CLI;
using Moq;

namespace Diary.Tests;

public class ArgParserTests
{
    private Mock<ICliController> _mockController;
    private IArgParser _argParser;

    [SetUp]
    public void SetUp()
    {
        _mockController = new Mock<ICliController>();
        _argParser = new ArgParser(_mockController.Object);
    }

    [Test]
    public void TestInitThrows()
    {
        Assert.Throws<ArgumentNullException>(() => { _ = new ArgParser(null!); });
    }

    [Test]
    public void TestLogAlias()
    {
        _argParser.ParseAndInvoke(["log"]);
        _argParser.ParseAndInvoke(["entry"]);
        _mockController.Verify(x => x.Log(), Times.Exactly(2));
    }

    [TestCaseSource(nameof(ParseReadFromTestCases))]
    public void TestRead(string[] cliArgs, string?[] expectedCommands)
    {
        _argParser.ParseAndInvoke(cliArgs);
        _mockController.Verify(x => x.ReplayFrom(expectedCommands), Times.Once);
    }

    [Test]
    public void TestReadNotCalled()
    {
        _argParser.ParseAndInvoke(["read"]);
        _mockController.VerifyNoOtherCalls();

        _argParser.ParseAndInvoke(["read", "from"]);
        _mockController.VerifyNoOtherCalls();        
    }

    [Test]
    public void TestSearchAlias()
    {
        _argParser.ParseAndInvoke(["search"]);
        _argParser.ParseAndInvoke(["find"]);
        _mockController.Verify(x => x.Search(Array.Empty<string>(), false), Times.Exactly(2));
    }

    [Test]
    public void TestSearchStrict()
    {
        _argParser.ParseAndInvoke(["search", "--strict"]);
        _argParser.ParseAndInvoke(["find", "--strict"]);
        _mockController.Verify(x => x.Search(Array.Empty<string>(), true), Times.Exactly(2));
    }

    [Test]
    public void TestBackup()
    {
        _argParser.ParseAndInvoke(["backup"]);
        _argParser.ParseAndInvoke(["backup", "testFilename.txt"]);
        _mockController.Verify(x => x.Backup(null), Times.Once);
        _mockController.Verify(x => x.Backup("testFilename.txt"), Times.Once);
    }

    [Test]
    public void TestExportNotCalled()
    {
        _argParser.ParseAndInvoke(["export"]);
        _mockController.VerifyNoOtherCalls();
    }

    [Test]
    public void TestExport()
    {
        _argParser.ParseAndInvoke(["export", "txt"]);
        _argParser.ParseAndInvoke(["export", "csv"]);
        _argParser.ParseAndInvoke(["export", "json"]);
        _mockController.Verify(x => x.Export("txt", It.IsAny<string>()), Times.Exactly(1));
        _mockController.Verify(x => x.Export("csv", It.IsAny<string>()), Times.Exactly(1));
        _mockController.VerifyNoOtherCalls();
        
        _argParser.ParseAndInvoke(["export", "stringstring"]);
        _mockController.Verify(x => x.Export("txt", It.IsAny<string>()), Times.Exactly(1));
    }

    private static IEnumerable<TestCaseData> ParseReadFromTestCases()
    {
        List<TestCaseData<string[], string?[]>> cases = [
            new (["read", "from", "2025", "january", "12"], ["2025", "january", "12"]),
            new (["read", "from", "2026", "feb", "1"], ["2026", "feb", "1"]),
            new (["read", "from", "12", "august", "2002"], ["12", "august", "2002"]),
            new (["read", "from", "2027"], ["2027", null, null]),
            new (["read", "from", "mar"], ["mar", null, null]),
            new (["read", "from", "12"], ["12", null, null]),
            new (["read", "from", "12", "march"], ["12", "march", null]),
            new (["read", "from", "march", "12"], ["march", "12", null]),
            new (["read", "from", "2012", "march"], ["2012", "march", null]),
            new (["read", "from", "march", "2012"], ["march", "2012", null]),
            new (["read", "from", "12", "2012"], ["12", "2012", null]),
            new (["read", "from", "2012", "12"], ["2012", "12", null]),
        ];

        foreach (var testCase in cases)
        {
            yield return testCase;
        }
    }
}