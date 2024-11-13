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
    public double? Weight { get; set; }
    public string Pronounciation { get; set; }
    public string Romanji { get; set; }
    public List<UserWordRelation> UserRelations { get; set; }
    public List<WordCharacterRelation> CharacterRelations { get; set; }
    public static async Task SetEntity(List<Character> charlist, string json, KtContext context, KawazuConverter converter)
    {
        List<string> writtenWord = [];
        Dictionary<Character, List<Word>> relationPairs = [];
        List<Word> words = [];
        var wordDataDict = JsonSerializer.Deserialize<KanjiData>(json) ?? throw new NullReferenceException("Failed to extract data");
        foreach (var character in charlist)
        {
            if (!wordDataDict.Words.TryGetValue(character.Char, out var value)) continue;
            foreach (var wordData in value)
            {
                var meanings = "";
                wordData.Meanings.ForEach(meaning =>
                {
                    meanings += string.Join(", ", meaning.Glosses) + ", ";
                });
                foreach (var variant in wordData.Variants.Where(variant => variant.Written.Contains(character.Char)))
                {
                    if (!writtenWord.Contains(variant.Written))
                    {
                        var chars = variant.Written.ToCharArray();
                        List<int> grades = [];
                        for (var i = 0; i < chars.Length; i++)
                        {
                            var grade = 5;
                            var selectedChar = context.Characters.Where(e => e.Char == chars[i].ToString()).AsNoTracking().FirstOrDefault();
                            if (selectedChar != null) grade = selectedChar.JLPT;
                            grades.Add(grade);
                        }
                        var romanji = variant.Pronounced == "" ? "" : await converter.Convert(variant.Pronounced, To.Romaji, Mode.Spaced, RomajiSystem.Hepburn);
                        Word word = new()
                        {
                            JLPT = grades.Min(),
                            Description = meanings,
                            Pronounciation = variant.Pronounced,
                            Romanji = romanji,
                            Written = variant.Written
                        };
                        writtenWord.Add(variant.Written);
                        words.Add(word);
                        if (relationPairs.ContainsKey(character))
                        {
                            relationPairs[character].Add(word);
                        }
                        else
                        {
                            relationPairs.Add(character, [word]);
                        }
                    }
                    else
                    {
                        var existingWord = words.FirstOrDefault(e => e.Written == variant.Written);
                        if (relationPairs.ContainsKey(character))
                        {
                            relationPairs[character].Add(existingWord);
                        }
                        else
                        {
                            relationPairs.Add(character, [existingWord]);
                        }
                    }
                }
            };
        }
        await context.Words.AddRangeAsync(words);
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
        foreach (var word in words)
        {
            word.Weight = GetWordWeight(word, context);
        }
        context.UpdateRange(words);
        await context.SaveChangesAsync();
    }
    private static double GetWordWeight(Word word, KtContext context)
    {
        var characterFrequecies = context.WordCharacterRelations
                                        .Where(wcr => wcr.WordId == word.Id)
                                        .Include(wcr => wcr.Char)
                                        .Select(wcr => wcr.Char.Freq)
                                        .ToList();
        var sumInverseFrequency = characterFrequecies.Sum(f => 1.0 / f);
        return sumInverseFrequency;
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