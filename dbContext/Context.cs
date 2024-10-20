using Kanji_teacher_backend.models;
using Microsoft.EntityFrameworkCore;

namespace Kanji_teacher_backend.dbContext;

public class KTContext : DbContext
{
    public DbSet<Character> Characters { get; set; }
    public DbSet<Word> Words { get; set; }
    public DbSet<UserTable> Users { get; set; }
    public DbSet<UserCharacterRelation> UserCharacterRelations { get; set; }
    public DbSet<UserWordRelation> UserWordRelations { get; set; }
    public DbSet<WordCharacterRelation> WordCharacterRelations { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var databaseLocation = Environment.GetEnvironmentVariable("DATABASE_LOCATION") ?? "/var/data/KanjiTeacherDatabase.db";
        optionsBuilder.UseSqlite(
            $"Data Source={databaseLocation}"
        );
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
    }
}