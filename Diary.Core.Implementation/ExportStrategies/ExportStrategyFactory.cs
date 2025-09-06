using Diary.Core;
using Diary.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Diary.Implementation.ExportStrategies;

public class ExportStrategyFactory : IExportStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ExportStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IExportStrategy CreateExporter(ExportOption exportOption)
    {
        return exportOption switch
        {
            ExportOption.Text => _serviceProvider.GetRequiredService<TextExportStrategy>(),
            // ExportOption.Json => expr,
            // ExportOption.Csv => expr,
            _ => throw new NotImplementedException(),
        };
    }
}