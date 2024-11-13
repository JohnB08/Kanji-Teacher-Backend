using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kanji_teacher_backend.dbContext;
using Kawazu;

namespace Kanji_teacher_backend.models;

public class Character
{
    [Key]
    public int Id { get; init; }
    [MaxLength(50)]
    public required string Char { get; init; }
    public int Grade { get; init; }
    public int JLPT { get; init; }
    public int Freq { get; init; }
    [MaxLength(100)]
    public string? KunReadings { get; init; }
    [MaxLength(100)]
    public string? KunRomanji { get; init; }
    [MaxLength(100)]
    public string? OnReadings { get; init; }
    [MaxLength(100)]
    public string? OnRomanji { get; init; }
    [MaxLength(200)]
    public required string Description { get; set; }
    public List<UserCharacterRelation> CharacterRelations { get; init; }
    public List<WordCharacterRelation> WordRelations { get; init; }


    /// <summary>
    /// Function to Set a new character in the database. 
    /// </summary>
    /// <param name="json"> String representing the json object fetched from the KanjiDev API.</param>
    /// <param name="context"> The server's database context. </param>
    /// <param name="converter"> KawazuConverter to convert text to kana and vise versa </param>
    /// <exception cref="NullReferenceException">If the json deserialization returns a null object. Throw.</exception>
    public static async Task SetEntities(string json, KtContext context, KawazuConverter converter)
    {
        var entities = JsonSerializer.Deserialize<Dictionary<string, KanjiInfo>>(json) ?? throw new NullReferenceException("Missing data in JSON");
        foreach (var entity in entities)
        {
            var onReadings = string.Join(", ", entity.Value.OnReadings);
            var kunReadings = string.Join(", ", entity.Value.KunReadings);
            var onRomanji = onReadings == "" ? null : await converter.Convert(onReadings, To.Romaji, Mode.Spaced, RomajiSystem.Hepburn);
            var kunRomanji = kunReadings == "" ? null : await converter.Convert(kunReadings, To.Romaji, Mode.Spaced, RomajiSystem.Hepburn);
            Character newChar = new()
            {
                Char = entity.Key,
                Grade = entity.Value.Grade ?? 0,
                JLPT = entity.Value.JLPT ?? 0,
                Freq = entity.Value.Freq ?? 3000,
                KunReadings = kunReadings,
                OnReadings = onReadings,
                KunRomanji = kunRomanji,
                OnRomanji = onRomanji,
                Description = string.Join(", ", entity.Value.Description)
            };
            context.Characters.Add(newChar);
            await context.SaveChangesAsync();
        }
    }
}
public class KanjiInfo
{
    [JsonPropertyName("grade")]
    public int? Grade { get; set; }
    [JsonPropertyName("freq")]
    public int? Freq { get; set; }
    [JsonPropertyName("jlpt_new")]
    public int? JLPT { get; set; }
    [JsonPropertyName("meanings")]
    public required List<string> Description { get; set; }
    [JsonPropertyName("readings_kun")]
    public required List<string> KunReadings { get; set; }
    [JsonPropertyName("readings_on")]
    public required List<string> OnReadings { get; set; }
}