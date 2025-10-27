using Diary.Models;

namespace Diary.Core;

public interface IExporterFactory
{
    public IExportStrategy CreateExporter(ExportOption exportOption);
}