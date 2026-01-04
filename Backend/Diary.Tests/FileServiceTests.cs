using Diary.Core;
using Diary.Implementation;
using Microsoft.Extensions.Options;
using Moq;

namespace Diary.Tests;

public class FileServiceTests
{
    private Mock<IOptions<AppConfigs>> _optionsMock;

    [SetUp]
    public void Setup()
    {
        _optionsMock = new Mock<IOptions<AppConfigs>>();
    }

    [Test]
    public void TestInitThrows()
    {
        Assert.Throws<ArgumentNullException>(() => { _ = new FileService(null!); });
    }

    [Test]
    public void TestBackup()
    {
        _optionsMock.Setup(x => x.Value).Returns(new AppConfigs { SqlitePath = "test.sqlite" });

        var dateString = DateTime.Now.ToString("yyyyMMddHHmm");
        var backupFile = "diarybackup_" + dateString + ".sqlite";
        
        var fileService = new FileService(_optionsMock.Object);
        fileService.Backup(null);
        var result = File.Exists(backupFile);
        Assert.That(result, Is.True);

        if (result)
            File.Delete(backupFile);
    }
}