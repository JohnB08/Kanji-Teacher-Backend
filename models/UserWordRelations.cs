using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kanji_teacher_backend.models;


public partial class UserWordRelation
{
    [Key]
    public int Id { get; init; }
    [ForeignKey("User")]
    public int UserId { get; init; }
    [ForeignKey("Word")]
    public int CharId { get; init; }
    public UserTable User { get; init; }
    public Word Word { get; init; }
    public int TimesCompleted { get; set; }
    public int TimesAttempted { get; set; }
}