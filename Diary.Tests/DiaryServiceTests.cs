using Diary.Core;
using Diary.Data;
using Diary.Implementation;
using Diary.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Diary.Tests;

public class DiaryServiceTests
{
    private string _fileName;
    private DiaryDbContext _diaryDbContext;
    private Mock<IFileService> _fileManagerMock;
    private Mock<IExportStrategyFactory> _exportStrategyFactoryMock;
    
    [SetUp]
    public void Setup()
    {
        _fileName = "test.txt";
        if (File.Exists(_fileName)) File.Delete(_fileName);

        _fileManagerMock = new Mock<IFileService>();

        // Use in-memory database instead of mocking
        var options = new DbContextOptionsBuilder<DiaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _diaryDbContext = new DiaryDbContext(options);

        _exportStrategyFactoryMock = new Mock<IExportStrategyFactory>();
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_fileName)) File.Delete(_fileName);
        _diaryDbContext.Dispose();
    }

    [Test]
    public void TestInit()
    {
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        Assert.That(diary.All(), Is.EqualTo(Enumerable.Empty<Entry>()), "Expected Blank list of diary entries on init");
    }

    [Test]
    public void TestBlankInit_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => { _ = new DiaryService(null!, _diaryDbContext, _exportStrategyFactoryMock.Object); });
        Assert.Throws<ArgumentNullException>(() => { _ = new DiaryService(_fileManagerMock.Object, null!, _exportStrategyFactoryMock.Object); });
        Assert.Throws<ArgumentNullException>(() => { _ = new DiaryService(_fileManagerMock.Object, _diaryDbContext, null!); });
    }

    [Test]
    public void TestAddEntryCalls()
    {
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        var entry = new Entry();
        diary.AddEntry(entry);
        
        // Verify the entry was added to the database
        var savedEntry = _diaryDbContext.Entries.FirstOrDefault();
        Assert.That(savedEntry, Is.Not.Null, "Entry should be saved to database");
    }

    [Test]
    public void TestSearch()
    {
        string s1 = "my", s2 = "name", s3 = "is";
        var entry1 = new Entry($"{s1}\n", DateTime.Now, Enumerable.Repeat(1d, s1.Length));
        var entry2 = new Entry($"{s2}\n", DateTime.Now, Enumerable.Repeat(1d, s2.Length));
        var entry3 = new Entry($"{s3}\n", DateTime.Now, Enumerable.Repeat(1d, s3.Length));
        var entry4 = new Entry($"{s1} {s2} {s3}\n", DateTime.Now, Enumerable.Repeat(1d, s1.Length + s3.Length + s3.Length + 3));

        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.AddEntry(entry1, entry2, entry3, entry4);
        var result1 = diary.Search(false, s1, s2).ToList();
        var result2 = diary.Search(true, s1, s2).ToList();
        
        Assert.That(result1, Has.Count.EqualTo(3), "Expected 3 entries");
        Assert.That(result1, Does.Contain(entry1));
        Assert.That(result1, Does.Contain(entry2));
        Assert.That(result1, Does.Contain(entry4));
        
        Assert.That(result2, Has.Count.EqualTo(1), "Expected 1 entry");
        Assert.That(result2, Does.Contain(entry4));

    }

    [Test]
    public void TestFilterEmptyArgs()
    {
        var entry = new Entry("bleh\n", DateTime.Now, Enumerable.Repeat(1d, 5));
        
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.AddEntry(entry);
        var result = diary.Filter(null, null, null);
        
        Assert.That(result, Is.EqualTo((List<Entry>)[entry]), "All entries when filters are empty");
    }

    [Test]
    public void TestFilterYearAlone()
    {
        DateTime t1 = DateTime.Now, t2 = DateTime.Now, t3 = DateTime.Now;
        t1 -= TimeSpan.FromDays(30);
        t2 -= TimeSpan.FromDays(70);
        t3 -= TimeSpan.FromDays(1000);
        var e1 = new Entry("bleh\n", t1, Enumerable.Repeat(1d, 5));
        var e2 = new Entry("bleh\n", t2, Enumerable.Repeat(1d, 5));
        var e3 = new Entry("bleh\n", t3, Enumerable.Repeat(1d, 5));
        List<Entry> expectedEntries = [e1, e2];

        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.AddEntry(e1, e2, e3);
        var result = diary.Filter(t1.Year, null, null);
        Assert.That(result, Is.EqualTo(expectedEntries), "Expected entries of a year were not filtered");
    }

    [Test]
    public void TestFilterMonthAlone()
    {
        DateTime t1 = DateTime.Now, t2 = DateTime.Now, t3 = DateTime.Now;
        t1 -= TimeSpan.FromDays(365);
        t3 -= TimeSpan.FromDays(1000);
        var e1 = new Entry("bleh\n", t1, Enumerable.Repeat(1d, 5));
        var e2 = new Entry("bleh\n", t2, Enumerable.Repeat(1d, 5));
        var e3 = new Entry("bleh\n", t3, Enumerable.Repeat(1d, 5));
        List<Entry> expectedEntries = [e1, e2];

        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.AddEntry(e1, e2, e3);

        var result = diary.Filter(null, t1.Month, null);
        Assert.That(result, Is.EqualTo(expectedEntries), "Expected entries of a month were not filtered");
    }

    [Test]
    public void TestFilterDateAlone()
    {
        TimeOnly t = new(12, 12, 12);
        DateOnly t1 = new(2015, 5, 7), t2 = new(2016, 7, 7), t3 = new(2018, 2, 8);
        var e1 = new Entry("bleh\n", t1.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e2 = new Entry("bleh\n", t2.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e3 = new Entry("bleh\n", t3.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        List<Entry> expectedEntries = [e1, e2];

        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.AddEntry(e1, e2, e3);
        var result = diary.Filter(null, null, t1.Day);
        Assert.That(result, Is.EqualTo(expectedEntries), "Expected entries of a date were not filtered");
    }

    [Test]
    public void TestFilterYearAndMonth()
    {
        TimeOnly t = new(12, 12, 12);
        DateOnly t1 = new(2015, 5, 7), t2 = new(2015, 5, 9), t3 = new(2018, 5, 8), t4 = new(2015, 8, 1);
        
        var e1 = new Entry("bleh\n", t1.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e2 = new Entry("bleh\n", t2.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e3 = new Entry("bleh\n", t3.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e4 = new Entry("bleh\n", t4.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        List<Entry> expectedEntries = [e1, e2];

        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.AddEntry(e1, e2, e3, e4);

        var result = diary.Filter(t1.Year, t1.Month, null);
        Assert.That(result, Is.EqualTo(expectedEntries), "Expected entries of a year and month were not filtered");
    }

    [Test]
    public void TestFilterYearAndDate()
    {
        TimeOnly t = new(12, 12, 12);
        DateOnly t1 = new(2015, 5, 7), t2 = new(2015, 9, 7), t3 = new(2018, 5, 7), t4 = new(2015, 8, 6);
        
        var e1 = new Entry("bleh\n", t1.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e2 = new Entry("bleh\n", t2.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e3 = new Entry("bleh\n", t3.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e4 = new Entry("bleh\n", t4.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        List<Entry> expectedEntries = [e1, e2];

        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.AddEntry(e1, e2, e3, e4);

        var result = diary.Filter(t1.Year, null, t1.Day);
        Assert.That(result, Is.EqualTo(expectedEntries), "Expected entries of a year and day were not filtered");
    }

    [Test]
    public void TestFilterMonthAndDate()
    {
        TimeOnly t = new(12, 12, 12);
        DateOnly t1 = new(2015, 5, 7), t2 = new(2018, 5, 7), t3 = new(2019, 5, 1), t4 = new(2015, 8, 7);
        
        var e1 = new Entry("bleh\n", t1.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e2 = new Entry("bleh\n", t2.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e3 = new Entry("bleh\n", t3.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e4 = new Entry("bleh\n", t4.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        List<Entry> expectedEntries = [e1, e2];

        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.AddEntry(e1, e2, e3, e4);

        var result = diary.Filter(null, t1.Month, t1.Day);
        Assert.That(result, Is.EqualTo(expectedEntries), "Expected entries of a month and day were not filtered");
    }

    [Test]
    public void TestAll()
    {
        TimeOnly t = new(12, 12, 12);
        DateOnly t1 = new(2015, 5, 7), t2 = new(2018, 5, 7), t3 = new(2019, 5, 1), t4 = new(2015, 8, 7);

        var e1 = new Entry("bleh\n", t1.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e2 = new Entry("bleh\n", t2.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e3 = new Entry("bleh\n", t3.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e4 = new Entry("bleh\n", t4.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));

        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.AddEntry(e1, e2, e3, e4);
        var result = diary.All();
        Assert.That(result, Is.EqualTo(new[] { e1, e2, e3, e4 }));
    }

    [Test]
    public void TestLastEntry()
    {
        TimeOnly t = new(12, 12, 12);
        DateOnly t1 = new(2015, 5, 7);
        DateOnly t2 = new(2018, 5, 7);
        DateOnly t3 = new(2019, 5, 1);
        DateOnly t4 = new(2015, 8, 7);

        var e1 = new Entry("bleh\n", t1.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e2 = new Entry("bleh\n", t2.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e3 = new Entry("bleh\n", t3.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));
        var e4 = new Entry("bleh\n", t4.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1d, 5));

        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.AddEntry(e1, e2, e3, e4);
        var result = diary.LastEntry();
        Assert.That(result, Is.EqualTo(e3));
    }

    [Test]
    public void TestBackup()
    {
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.Backup("test");
        _fileManagerMock.Verify(x => x.Backup("test"), Times.Once);
    }

    [Test]
    public void TestExport()
    {
        var mockExportStrategy = new Mock<IExportStrategy>();
        _exportStrategyFactoryMock
            .Setup(x => x.CreateExporter(ExportOption.Text))
            .Returns(mockExportStrategy.Object);
        
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContext, _exportStrategyFactoryMock.Object);
        diary.Export(ExportOption.Text, "test");
        _exportStrategyFactoryMock.Verify(x => x.CreateExporter(ExportOption.Text), Times.Once);
        mockExportStrategy.Verify(x => x.Export(It.IsAny<List<Entry>>(), "test"));
    }
}
