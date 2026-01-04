using Diary.Implementation.ExportStrategies;

namespace Diary.Tests;

public class ExporterFactoryTests
{
    [Test]
    public void TestInitThrows()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new ExporterFactory(null!);
        });
    }
}