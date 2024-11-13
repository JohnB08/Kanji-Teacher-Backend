
using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;
using Kawazu;
using Microsoft.AspNetCore.Mvc;

namespace Kanji_teacher_backend.Controllers.SetupControllers;


[ApiController]
[Route("api")]
public class PopulateKanji(KtContext context, KawazuConverter converter) : ControllerBase
{
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
            const string kanjiFilePath = "./raw/kanji.json";
            const string wordFilePath = "./raw/kanjiapi_full.json";
            var kanjiJson = await System.IO.File.ReadAllTextAsync(kanjiFilePath);
            var wordJson = await System.IO.File.ReadAllTextAsync(wordFilePath);
            await Character.SetEntities(json: kanjiJson, context: context, converter: converter);
            var characters = context.Characters.ToList();
            await Word.SetEntity(charlist: characters, json: wordJson, context: context, converter: converter);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Something went wrong fetching kanji: {ex.Message}" });
        }
    }
}
