using Kanji_teacher_backend.models;
using Microsoft.EntityFrameworkCore;

namespace Kanji_teacher_backend.dbContext;

public class KTContext : DbContext
{
    public DbSet<Character> Characters { get; set; }
    public DbSet<UserTable> Users { get; set; }
    public DbSet<UserCharacterRelation> UserCharacterRelations { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
            "Data Source=KanjiTeacherDatabase.db"
        );
    }
}