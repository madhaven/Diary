using Diary.Core;
using Diary.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Diary.Implementation.Export;

public class ExporterFactory : IExporterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ExporterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IExportStrategy CreateExporter(ExportOption exportOption)
    {
        return exportOption switch
        {
            ExportOption.Text => _serviceProvider.GetRequiredService<TextExporter>(),
            // ExportOption.Json => expr,
            // ExportOption.Csv => expr,
            _ => throw new NotImplementedException(),
        };
    }
}