using Diary.Implementation.ExportStrategies;

namespace Diary.Tests;

public class ExportStrategyFactoryTests
{
    [Test]
    public void TestInitThrows()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new ExportStrategyFactory(null!);
        });
    }
}