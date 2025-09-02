using Diary.Models;

namespace Diary.Core;

public interface IFileService
{
    public void Backup(string? opName);
}