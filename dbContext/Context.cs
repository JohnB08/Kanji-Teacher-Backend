using Kanji_teacher_backend.models;
using Microsoft.EntityFrameworkCore;

namespace Kanji_teacher_backend.dbContext;

public class KtContext : DbContext
{
    public DbSet<Character> Characters { get; init; }
    public DbSet<Word> Words { get; init; }
    public DbSet<UserTable> Users { get; init; }
    public DbSet<UserCharacterRelation> UserCharacterRelations { get; init; }
    public DbSet<UserWordRelation> UserWordRelations { get; init; }
    public DbSet<WordCharacterRelation> WordCharacterRelations { get; init; }
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