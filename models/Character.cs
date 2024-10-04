using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kanji_teacher_backend.dbContext;

namespace Kanji_teacher_backend.models;

public partial class Character
{
    [Key]
    public int Id { get; set; }
    public required string Char { get; set; }
    public int Grade { get; set; }
    public string? KunReadings { get; set; }
    public string? Meanings { get; set; }
    public string? OnReadings { get; set; }
    public required string Description { get; set; }
    public List<UserCharacterRelation> Relations { get; set; }

    /// <summary>
    /// Function to Set a new character in the database. 
    /// </summary>
    /// <param name="json"> String representing the json object fetched from the KanjiDev API.</param>
    /// <param name="context"> The server's database context. </param>
    /// <exception cref="NullReferenceException">If the json deserialization returns a null object. Throw.</exception>
    public static void SetEntity(string json, KTContext context)
    {
        var baseEntity = JsonSerializer.Deserialize<KanjiDevJson>(json) ?? throw new NullReferenceException("Missing data in JSON");
        Character entity = new()
        {
            Grade = baseEntity.Grade,
            Char = baseEntity.Char,
            Description = baseEntity.Description,
            KunReadings = string.Join(",", baseEntity.KunReadings),
            OnReadings = string.Join(",", baseEntity.OnReadings),
            Meanings = string.Join(",", baseEntity.Meanings)
        };
        context.Characters.Add(entity);
        context.SaveChanges();
        return;
    }
}
public class KanjiDevJson
{
    [JsonPropertyName("grade")]
    public int Grade { get; set; }
    [JsonPropertyName("heisig_en")]
    public required string Description { get; set; }
    [JsonPropertyName("kanji")]
    public required string Char { get; set; }
    [JsonPropertyName("kun_readings")]
    public required List<string> KunReadings { get; set; }
    [JsonPropertyName("meanings")]
    public required List<string> Meanings { get; set; }
    [JsonPropertyName("on_readings")]
    public required List<string> OnReadings { get; set; }
}