using System.ComponentModel.DataAnnotations;

namespace Kanji_teacher_backend.models;

public partial class UserTable
{
    [Key]
    public int Id { get; init; }
    public required string Uid { get; init; }
    public required int MaxGrade { get; set; }
    public required int Xp { get; set; }
    public List<UserCharacterRelation> CharacterRelations { get; init; }
    public List<UserWordRelation> WordRelations { get; init; }
}