using System.Text.Json;
using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;
using Microsoft.AspNetCore.Mvc;

namespace Kanji_teacher_backend.Controllers.SetupControllers;


[ApiController]
[Route("api")]
public class PopulateKanji : ControllerBase
{
    private readonly KTContext _context;
    public PopulateKanji(KTContext context)
    {
        _context = context;
    }
    /// <summary>
    /// Controller to populate the database with Kanji symbols from Kanjiapi.dev.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">throws if it cannot find the initial list of chars.</exception>
    [HttpGet("pull_kanji")]
    public async Task<IActionResult> FetchKanji()
    {
        try
        {
            string BaseUrl = "https://kanjiapi.dev/v1/kanji/";
            int MaxGrade = 8;
            HttpClient client = new HttpClient();
            List<string> Chars = [];
            for (int i = 1; i <= MaxGrade; i++)
            {
                if (i == 7) continue;
                var result = await client.GetAsync(BaseUrl + $"grade-{i}");
                result.EnsureSuccessStatusCode();
                var json = await result.Content.ReadAsStringAsync() ?? throw new NullReferenceException("Missing Json String after fetch.");
                var newChars = JsonSerializer.Deserialize<List<string>>(json) ?? throw new NullReferenceException("Failed to parse Json.");
                Chars.AddRange(newChars);
            }
            for (int i = 0; i < Chars.Count; i++)
            {
                var result = await client.GetAsync(BaseUrl + $"{Chars[i]}");
                result.EnsureSuccessStatusCode();
                var json = await result.Content.ReadAsStringAsync();
                Character.SetEntity(json, _context);
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Something went wrong fetching kanji: {ex.Message}" });
        }
    }
}