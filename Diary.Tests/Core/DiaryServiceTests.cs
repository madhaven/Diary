using Diary.Core;
using Diary.Data;
using Diary.Implementation;
using Diary.Models;
using Moq;

namespace Diary.Tests.Core;

public class DiaryServiceTests
{
    private string _fileName;
    private Mock<DiaryDbContext> _diaryDbContextMock;
    private Mock<IFileService> _fileManagerMock;
    
    [SetUp]
    public void Setup()
    {
        _fileName = "test.txt";
        if (File.Exists(_fileName)) File.Delete(_fileName);
        
        _fileManagerMock = new Mock<IFileService>();
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_fileName)) File.Delete(_fileName);
    }

    [Test]
    public void TestInit()
    {
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContextMock.Object);

        Assert.That(diary.All(), Is.EqualTo(Enumerable.Empty<Entry>()), "Expected Blank list of diary entries on init");
        // TODO: assert file manager versions?
    }

    [Test]
    public void TestBlankInit_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => { _ = new DiaryService(null!, _diaryDbContextMock.Object); });
        Assert.Throws<ArgumentNullException>(() => { _ = new DiaryService(_fileManagerMock.Object, null!); });
    }

    [Test]
    public void TestAddEntryCallsFileManager()
    {
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContextMock.Object);
        diary.AddEntry(new Entry());
        _fileManagerMock.Verify(x => x.Write(It.IsAny<Entry[]>()), Times.Once);
    }

    // TODO: check file init specs TestFirstEntry();

    /* TODO: check second entry format
    [Test]
    public void TestAddEntry()
    {
        var diary = new DiaryService.Core.DiaryService(_fileManagerMock.Object);
        var time1 = DateTime.Now;
        diary.AddEntry(new Entry("hello\n", time1, Enumerable.Repeat(1, 6)));
        var time2 = DateTime.Now;
        diary.AddEntry(new Entry("olleh\n", time2, Enumerable.Repeat(2, 6)));
    } */
    
    // TODO: test load

    [Test]
    public void TestFilterEmptyArgs()
    {
        var entry = new Entry("bleh\n", DateTime.Now, Enumerable.Repeat(1, 5));
        _fileManagerMock.Setup(x => x.Load()).Returns([entry]);
        
        var diary =  new DiaryService(_fileManagerMock.Object, _diaryDbContextMock.Object);
        diary.AddEntry(entry);
        var result = diary.Filter(null, null, null);
        
        Assert.That(result, Is.EqualTo((List<Entry>)[entry]), "All entries when filters are empty");
    }

    [Test]
    public void TestFilterYearAlone()
    {
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContextMock.Object);
        DateTime t1 = DateTime.Now, t2 = DateTime.Now, t3 = DateTime.Now;
        t1 -= TimeSpan.FromDays(30);
        t2 -= TimeSpan.FromDays(70);
        t3 -= TimeSpan.FromDays(1000);
        var e1 = new Entry("bleh\n", t1, Enumerable.Repeat(1, 5));
        var e2 = new Entry("bleh\n", t2, Enumerable.Repeat(1, 5));
        var e3 = new Entry("bleh\n", t3, Enumerable.Repeat(1, 5));
        _fileManagerMock.Setup(x => x.Load()).Returns([e1, e2, e3]);
        
        diary.AddEntry(e1, e2, e3);
        var result = diary.Filter(t1.Year, null, null);
        Assert.That(result, Is.EqualTo((List<Entry>)[e1, e2]), "Expected entries of a year were not filtered");
    }

    [Test]
    public void TestFilterMonthAlone()
    {
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContextMock.Object);
        DateTime t1 = DateTime.Now, t2 = DateTime.Now, t3 = DateTime.Now;
        t1 -= TimeSpan.FromDays(365);
        t3 -= TimeSpan.FromDays(1000);
        var e1 = new Entry("bleh\n", t1, Enumerable.Repeat(1, 5));
        var e2 = new Entry("bleh\n", t2, Enumerable.Repeat(1, 5));
        var e3 = new Entry("bleh\n", t3, Enumerable.Repeat(1, 5));
        _fileManagerMock.Setup(x => x.Load()).Returns([e1, e2, e3]);
        
        diary.AddEntry(e1, e2, e3);
        var result = diary.Filter(null, t1.Month, null);
        Assert.That(result, Is.EqualTo((List<Entry>)[e1, e2]), "Expected entries of a month were not filtered");
    }

    [Test]
    public void TestFilterDateAlone()
    {
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContextMock.Object);
        TimeOnly t = new(12, 12, 12);
        DateOnly t1 = new(2015, 5, 7), t2 = new(2016, 7, 7), t3 = new(2018, 2, 8);
        var e1 = new Entry("bleh\n", t1.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        var e2 = new Entry("bleh\n", t2.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        var e3 = new Entry("bleh\n", t3.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        _fileManagerMock.Setup(x => x.Load()).Returns([e1, e2, e3]);
        
        diary.AddEntry(e1, e2, e3);
        var result = diary.Filter(null, null, t1.Day);
        Assert.That(result, Is.EqualTo((List<Entry>)[e1, e2]), "Expected entries of a date were not filtered");
    }

    [Test]
    public void TestFilterYearAndMonth()
    {
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContextMock.Object);
        TimeOnly t = new(12, 12, 12);
        DateOnly t1 = new(2015, 5, 7), t2 = new(2015, 5, 9), t3 = new(2018, 5, 8), t4 = new(2015, 8, 1);
        
        var e1 = new Entry("bleh\n", t1.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        var e2 = new Entry("bleh\n", t2.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        var e3 = new Entry("bleh\n", t3.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        var e4 = new Entry("bleh\n", t4.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        _fileManagerMock.Setup(x => x.Load()).Returns([e1, e2, e3, e4]);
        
        diary.AddEntry(e1, e2, e3, e4);
        var result = diary.Filter(t1.Year, t1.Month, null);
        Assert.That(result, Is.EqualTo((List<Entry>)[e1, e2]), "Expected entries of a year and month were not filtered");
    }

    [Test]
    public void TestFilterYearAndDate()
    {
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContextMock.Object);
        TimeOnly t = new(12, 12, 12);
        DateOnly t1 = new(2015, 5, 7), t2 = new(2015, 9, 7), t3 = new(2018, 5, 7), t4 = new(2015, 8, 6);
        
        var e1 = new Entry("bleh\n", t1.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        var e2 = new Entry("bleh\n", t2.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        var e3 = new Entry("bleh\n", t3.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        var e4 = new Entry("bleh\n", t4.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        _fileManagerMock.Setup(x => x.Load()).Returns([e1, e2, e3, e4]);
        
        diary.AddEntry(e1, e2, e3, e4);
        var result = diary.Filter(t1.Year, null, t1.Day);
        Assert.That(result, Is.EqualTo((List<Entry>)[e1, e2]), "Expected entries of a year and day were not filtered");
    }

    [Test]
    public void TestFilterMonthAndDate()
    {
        var diary = new DiaryService(_fileManagerMock.Object, _diaryDbContextMock.Object);
        TimeOnly t = new(12, 12, 12);
        DateOnly t1 = new(2015, 5, 7), t2 = new(2018, 5, 7), t3 = new(2019, 5, 1), t4 = new(2015, 8, 7);
        
        var e1 = new Entry("bleh\n", t1.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        var e2 = new Entry("bleh\n", t2.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        var e3 = new Entry("bleh\n", t3.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        var e4 = new Entry("bleh\n", t4.ToDateTime(t, DateTimeKind.Local), Enumerable.Repeat(1, 5));
        _fileManagerMock.Setup(x => x.Load()).Returns([e1, e2, e3, e4]);
        
        diary.AddEntry(e1, e2, e3, e4);
        var result = diary.Filter(null, t1.Month, t1.Day);
        Assert.That(result, Is.EqualTo((List<Entry>)[e1, e2]), "Expected entries of a month and day were not filtered");
    }
    
    // TODO: test search.
}
