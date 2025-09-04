using Diary.Models;

namespace Diary.Core;

public interface IExportStrategyFactory
{
    public IExportStrategy CreateExporter(ExportOption exportOption);
}