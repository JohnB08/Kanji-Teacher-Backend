using System.ComponentModel.DataAnnotations;

namespace Kanji_teacher_backend.models;

public partial class UserTable
{
    [Key]
    public int Id { get; set; }
    public required string Uid { get; set; }
    public required int MaxGrade { get; set; }
    public required int Xp { get; set; }
    public List<UserCharacterRelation> CharacterRelations { get; set; }
    public List<UserWordRelation> WordRelations { get; set; }
}