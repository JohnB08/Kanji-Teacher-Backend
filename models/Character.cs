using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kanji_teacher_backend.dbContext;
using Kawazu;

namespace Kanji_teacher_backend.models;

public partial class Character
{
    [Key]
    public int Id { get; set; }
    public required string Char { get; set; }
    public int Grade { get; set; }
    public int JLPT { get; set; }
    public int Freq { get; set; }
    public string? KunReadings { get; set; }
    public string? KunRomanji { get; set; }
    public string? OnReadings { get; set; }
    public string? OnRomanji { get; set; }
    public required string Description { get; set; }
    public List<UserCharacterRelation> CharacterRelations { get; set; }
    public List<WordCharacterRelation> WordRelations { get; set; }


    /// <summary>
    /// Function to Set a new character in the database. 
    /// </summary>
    /// <param name="json"> String representing the json object fetched from the KanjiDev API.</param>
    /// <param name="context"> The server's database context. </param>
    /// <exception cref="NullReferenceException">If the json deserialization returns a null object. Throw.</exception>
    public static async Task SetEntities(string json, KTContext context, KawazuConverter converter)
    {
        var entities = JsonSerializer.Deserialize<Dictionary<string, KanjiInfo>>(json) ?? throw new NullReferenceException("Missing data in JSON");
        foreach (var entity in entities)
        {
            var OnReadings = string.Join(", ", entity.Value.OnReadings);
            var KunReadings = string.Join(", ", entity.Value.KunReadings);
            var OnRomanji = OnReadings == "" ? null : await converter.Convert(OnReadings, To.Romaji, Mode.Spaced, RomajiSystem.Hepburn);
            var KunRomanji = KunReadings == "" ? null : await converter.Convert(KunReadings, To.Romaji, Mode.Spaced, RomajiSystem.Hepburn);
            Character newChar = new()
            {
                Char = entity.Key,
                Grade = entity.Value.Grade ?? 0,
                JLPT = entity.Value.JLPT ?? 0,
                Freq = entity.Value.Freq ?? 3000,
                KunReadings = KunReadings,
                OnReadings = OnReadings,
                KunRomanji = KunRomanji,
                OnRomanji = OnRomanji,
                Description = string.Join(", ", entity.Value.Description)
            };
            context.Characters.Add(newChar);
            context.SaveChanges();
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