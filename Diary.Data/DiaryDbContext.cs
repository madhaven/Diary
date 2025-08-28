using Microsoft.EntityFrameworkCore;

namespace Diary.Data;

public class DiaryDbContext : DbContext
{
    public DiaryDbContext(DbContextOptions<DiaryDbContext> options) : base(options) {}

    public DbSet<EntryData> Entries { get; set; } // TODO: nullable needed ? remove comment if no errors.
}
