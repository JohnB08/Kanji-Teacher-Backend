using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kanji_teacher_backend.models;


public partial class UserCharacterRelation
{
    [Key]
    public int Id { get; init; }
    [ForeignKey("User")]
    public int UserId { get; init; }
    [ForeignKey("Char")]
    public int CharId { get; init; }
    public UserTable User { get; init; }
    public Character Char { get; init; }
    public int TimesCompleted { get; set; }
    public int TimesAttempted { get; set; }
}