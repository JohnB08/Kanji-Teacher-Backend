using System.Text.Json;
using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.Util;
using Microsoft.AspNetCore.Mvc;

namespace Kanji_teacher_backend.Controllers;

[ApiController]
[Route("api")]
public class FlashCardDataController : ControllerBase
{
    private readonly KTContext _context;
    private readonly FirebaseService _service;
    public FlashCardDataController(KTContext context, FirebaseService service)
    {
        _context = context;
        _service = service;
    }
    /// <summary>
    /// Gets the flash card data for the spesific uid.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getFlashCard")]
    public async Task<IActionResult> GetFlashCard()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized("Authorization header is missing or invalid");
        }
        var token = authHeader["Bearer ".Length..].Trim();
        var uid = await _service.ValidateFirebaseToken(token);
        if (uid == null)
        {
            return Unauthorized("Token is Invalid");
        }
        var currentUser = UserHandler.GetUser(uid, _context);
        var getQuestions = RelationHandler.GetRelationAndAnswers(currentUser, _context);
        var json = JsonSerializer.Serialize(getQuestions);
        return Ok(json);
    }
    /// <summary>
    /// validates the query param answer with the query param id and the uid in the token
    /// </summary>
    /// <param name="id">the id of the relation</param>
    /// <param name="answer">the user's answer</param>
    /// <returns></returns>
    [HttpGet("validateAnswer")]
    public async Task<IActionResult> ValidateAnswer([FromQuery] int id, [FromQuery] string answer)
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized("Authorization header is missing or invalid");
        }
        var token = authHeader["Bearer ".Length..].Trim();
        var uid = await _service.ValidateFirebaseToken(token);
        if (uid == null)
        {
            return Unauthorized("Token is Invalid");
        }
        var currentUser = UserHandler.GetUser(uid, _context);
        var validation = RelationHandler.ValidateAnswer(currentUser, _context, answer, id);
        var json = JsonSerializer.Serialize(validation);
        return Ok(json);
    }
}