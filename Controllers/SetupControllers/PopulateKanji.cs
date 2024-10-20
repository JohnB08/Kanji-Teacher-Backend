using System.Text.Json;
using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;
using Kawazu;
using Microsoft.AspNetCore.Mvc;

namespace Kanji_teacher_backend.Controllers.SetupControllers;


[ApiController]
[Route("api")]
public class PopulateKanji : ControllerBase
{
    private readonly KTContext _context;
    private readonly KawazuConverter _converter;
    public PopulateKanji(KTContext context, KawazuConverter converter)
    {
        _context = context;
        _converter = converter;
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
            var kanjiFilePath = "./raw/kanji.json";
            var wordFilePath = "./raw/kanjiapi_full.json";
            var kanjiJson = System.IO.File.ReadAllText(kanjiFilePath);
            var wordJson = System.IO.File.ReadAllText(wordFilePath);
            await Character.SetEntities(json: kanjiJson, context: _context, converter: _converter);
            List<Character> characters = _context.Characters.ToList();
            await Word.SetEntity(Chars: characters, Json: wordJson, context: _context, converter: _converter);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Something went wrong fetching kanji: {ex.Message}" });
        }
    }
}