using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kanji_teacher_backend.models;


public partial class WordCharacterRelation
{
    [Key]
    public int Id { get; init; }
    [ForeignKey("Char")]
    public int CharId { get; init; }
    [ForeignKey("Word")]
    public int WordId { get; init; }
    public Character Char { get; init; }
    public Word Word { get; init; }
}