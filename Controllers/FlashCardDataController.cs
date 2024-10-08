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
    public async Task<IActionResult> GetFlashCard([FromQuery] bool? progress)
    {
        try
        {
            /* Validate token existence */
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new
                {
                    message = "Authorization header is missing or invalid"
                });
            }
            var token = authHeader["Bearer ".Length..].Trim();
            /* Validate token on firebase */
            var uid = await _service.ValidateFirebaseToken(token);
            if (uid == null)
            {
                return Unauthorized(new
                {
                    message = $"Authorization header is missing or invalid {uid}"
                });
            }
            /* Fetch user associated with token */
            var currentUser = UserHandler.GetUser(uid, _context);
            /* If user wants to progress, upgrade Grade. */
            if (progress != null && progress == true)
            {
                ProgressHandler.UpgradeGrade(currentUser, _context);
            }
            /* Fetch new flashcard data, and return as json */
            var getQuestions = RelationHandler.GetRelationAndAnswers(currentUser, _context);
            var json = JsonSerializer.Serialize(getQuestions);
            return Ok(json);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = ex.Message
            });
        }
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
        try
        {
            /* Check for token */
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header is missing or invalid");
            }
            var token = authHeader["Bearer ".Length..].Trim();
            /* Validate against firebase */
            var uid = await _service.ValidateFirebaseToken(token);
            if (uid == null)
            {
                return Unauthorized("Token is Invalid");
            }
            /* Fetch user associated with token */
            var currentUser = UserHandler.GetUser(uid, _context);
            /* validate answer, and return response as json. */
            var validation = RelationHandler.ValidateAnswer(currentUser, _context, answer, id);
            var json = JsonSerializer.Serialize(validation);
            return Ok(json);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = ex.Message
            });
        }
    }
    [HttpGet("userinfo")]
    public async Task<IActionResult> GetUserInfo()
    {
        try
        {
            /* Check for token */
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header is missing or invalid");
            }
            var token = authHeader["Bearer ".Length..].Trim();
            /* Validate against firebase */
            var uid = await _service.ValidateFirebaseToken(token);
            if (uid == null)
            {
                return Unauthorized("Token is Invalid");
            }
            /* Fetch user associated with token */
            var currentUser = UserHandler.GetUser(uid, _context);
            var userStats = UserHandler.GetUserInfo(currentUser, _context);
            var json = JsonSerializer.Serialize(userStats);
            return Ok(json);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = $"Server error, something went wrong: {ex.Message}"
            });
        }
    }
}