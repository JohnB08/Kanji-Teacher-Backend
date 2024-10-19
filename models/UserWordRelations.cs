using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kanji_teacher_backend.models;


public partial class UserWordRelation
{
    [Key]
    public int Id { get; set; }
    [ForeignKey("User")]
    public int UserId { get; set; }
    [ForeignKey("Word")]
    public int CharId { get; set; }
    public UserTable User { get; set; }
    public Word Word { get; set; }
    public int TimesCompleted { get; set; }
    public int TimesAttempted { get; set; }
}