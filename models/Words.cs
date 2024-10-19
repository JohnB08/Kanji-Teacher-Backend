using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kanji_teacher_backend.dbContext;
using Kawazu;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Kanji_teacher_backend.models;


public partial class Word
{
    [Key]
    public int Id { get; set; }
    public int JLPT { get; set; }
    public string Description { get; set; }
    public string Written { get; set; }
    public string Pronounciation { get; set; }
    public string Romanji { get; set; }
    public List<UserWordRelation> UserRelations { get; set; }
    public List<WordCharacterRelation> CharacterRelations { get; set; }
    public static async Task SetEntity(List<Character> Chars, string Json, KTContext context, KawazuConverter converter)
    {
        List<string> writtenWord = [];
        Dictionary<Character, List<Word>> relationPairs = [];
        List<Word> Words = [];
        var WordDataDict = JsonSerializer.Deserialize<KanjiData>(Json) ?? throw new NullReferenceException("Failed to extract data");
        foreach (var Char in Chars)
        {
            if (WordDataDict.Words.TryGetValue(Char.Char, out KanjiWord[]? value))
            {
                var WordDatas = value;

                foreach (var WordData in WordDatas)
                {
                    string meanings = "";
                    WordData.Meanings.ForEach(meaning =>
                    {
                        meanings += string.Join(", ", meaning.Glosses) + ", ";
                    });
                    foreach (var variant in WordData.Variants)
                    {
                        if (variant.Written.Contains(Char.Char))
                        {
                            if (!writtenWord.Contains(variant.Written))
                            {
                                var chars = variant.Written.ToCharArray();
                                List<int> Grades = [];
                                for (int i = 0; i < chars.Count(); i++)
                                {
                                    int grade = 5;
                                    var selectedChar = context.Characters.Where(e => e.Char == chars[i].ToString()).AsNoTracking().FirstOrDefault();
                                    if (selectedChar != null) grade = selectedChar.JLPT;
                                    Grades.Add(grade);
                                }
                                var Romanji = variant.Pronounced == "" ? "" : await converter.Convert(variant.Pronounced, To.Romaji, Mode.Spaced, RomajiSystem.Hepburn);
                                Word word = new()
                                {
                                    JLPT = Grades.Min(),
                                    Description = meanings,
                                    Pronounciation = variant.Pronounced,
                                    Romanji = Romanji,
                                    Written = variant.Written
                                };
                                writtenWord.Add(variant.Written);
                                Words.Add(word);
                                if (relationPairs.ContainsKey(Char))
                                {
                                    relationPairs[Char].Add(word);
                                }
                                else
                                {
                                    relationPairs.Add(Char, [word]);
                                }
                            }
                            else
                            {
                                var existingWord = Words.FirstOrDefault(e => e.Written == variant.Written);
                                if (relationPairs.ContainsKey(Char))
                                {
                                    relationPairs[Char].Add(existingWord);
                                }
                                else
                                {
                                    relationPairs.Add(Char, [existingWord]);
                                }
                            }

                        }
                    }
                };
            }
        }
        await context.Words.AddRangeAsync(Words);
        foreach (var pair in relationPairs)
        {
            foreach (var word in pair.Value)
            {
                await context.WordCharacterRelations.AddAsync(new()
                {
                    Char = pair.Key,
                    Word = word
                });
            }
        };
        await context.SaveChangesAsync();
    }
}
public class KanjiWord
{
    [JsonPropertyName("meanings")]
    public List<Meaning> Meanings { get; set; }
    [JsonPropertyName("variants")]
    public List<Variant> Variants { get; set; }
}

public class Meaning
{
    [JsonPropertyName("glosses")]
    public List<string> Glosses { get; set; }
}

public class Variant
{
    [JsonPropertyName("priorities")]
    public List<string> Priorities { get; set; }
    [JsonPropertyName("pronounced")]
    public string Pronounced { get; set; }
    [JsonPropertyName("written")]
    public string Written { get; set; }
}

public class KanjiData
{
    [JsonPropertyName("words")]
    public Dictionary<string, KanjiWord[]> Words { get; set; }
}