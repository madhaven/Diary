using Diary.Implementation.ExportStrategies;
using Diary.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Diary.Tests;

public class ExportStrategyFactoryTests
{
    private Mock<IServiceProvider> _serviceProviderMock;

    [SetUp]
    public void Setup()
    {
        _serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
    }

    [Test]
    public void TestInitThrows()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new ExportStrategyFactory(null!);
        });
    }

    // TODO: [TestCaseSource(nameof(ExporterCases))]
    public void TestCreateExporter(ExportOption exportOption, Type type)
    {
        _serviceProviderMock.Setup(x => x.GetRequiredService<TextExportStrategy>()).Returns(new TextExportStrategy());
        var factory = new ExportStrategyFactory(_serviceProviderMock.Object);

        var strategy = factory.CreateExporter(exportOption);
        Assert.Equals(strategy.GetType(), type);
    }

    private static IEnumerable<TestCaseData> ExporterCases()
    {
        List<TestCaseData<ExportOption, Type>> cases = [
            new (ExportOption.Text, typeof(TextExportStrategy)),
        ];

        foreach (var testCase in cases)
        {
            yield return testCase;
        }
    }
}