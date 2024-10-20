using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kanji_teacher_backend.models;


public partial class UserCharacterRelation
{
    [Key]
    public int Id { get; set; }
    [ForeignKey("User")]
    public int UserId { get; set; }
    [ForeignKey("Char")]
    public int CharId { get; set; }
    public UserTable User { get; set; }
    public Character Char { get; set; }
    public int TimesCompleted { get; set; }
    public int TimesAttempted { get; set; }
}