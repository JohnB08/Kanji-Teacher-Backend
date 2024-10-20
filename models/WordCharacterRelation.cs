using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kanji_teacher_backend.models;


public partial class WordCharacterRelation
{
    [Key]
    public int Id { get; set; }
    [ForeignKey("Char")]
    public int CharId { get; set; }
    [ForeignKey("Word")]
    public int WordId { get; set; }
    public Character Char { get; set; }
    public Word Word { get; set; }
}