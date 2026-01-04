using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Diary.Data;

internal class DiaryDbContextFactory
    : IDesignTimeDbContextFactory<DiaryDbContext>
{
    public DiaryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DiaryDbContext>();
        optionsBuilder.UseSqlite("Data Source=diary.db");
        return new DiaryDbContext(optionsBuilder.Options);
    }
}