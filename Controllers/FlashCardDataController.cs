using System.Text.Json;
using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.Util;
using Kanji_Teacher_Backend.Util.Firebase;
using Microsoft.AspNetCore.Mvc;

namespace Kanji_teacher_backend.Controllers;

[ApiController]
[Route("api")]
public class FlashCardDataController(KtContext context, FirebaseService service) : ControllerBase
{
    /// <summary>
    /// Gets the flash card data for the spesific uid.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getFlashCard")]
    public async Task<IActionResult> GetFlashCard([FromQuery] bool? progress, [FromQuery] string mode)
    {
        try
        {
            /* Validate token existence */
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                var getQuestionsNoUser =
                    await Util.RelationHandler.UserCharacterRelationHandler.GetRelationAndAnswers(null, context);
                
                return Ok(getQuestionsNoUser);
            }
            var token = authHeader["Bearer ".Length..].Trim();
            /* Validate token on firebase */
            var uid = await service.ValidateFirebaseToken(token);
            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized(new
                {
                    message = $"Authorization header is missing or invalid {uid}"
                });
            }
            /* Fetch user associated with token */
            var currentUser = await Util.UserHandler.UserHandler.GetUser(uid, context);
            /* If user wants to progress, upgrade Grade. */
            if (progress is true)
            {
                await Util.ProgressHandler.ProgressHandler.UpgradeGrade(currentUser, context);
            }
            /* Fetch new flashcard data, and return as json */
            var getQuestions = mode switch
            {
                "character" => await Util.RelationHandler.UserCharacterRelationHandler.GetRelationAndAnswers(currentUser, context),
                "phrase" => await Util.RelationHandler.UserWordRelationshipHandler.GetRelationAndAnswers(currentUser, context),
                _ => throw new Exception($"Current mode not supported, {mode}")
            };
            return Ok(getQuestions);
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
    /// <param name="mode">the user's current "flashcard mode" phrase | character</param>
    /// <returns></returns>
    [HttpGet("validateAnswer")]
    public async Task<IActionResult> ValidateAnswer([FromQuery] int id, [FromQuery] string answer, [FromQuery] string mode)
    {
        try
        {
            /* Check for token */
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                var validnouser = await Util.RelationHandler.UserCharacterRelationHandler.ValidateAnswer(null, context, answer, id);
                return Ok(validnouser);
            }
            var token = authHeader["Bearer ".Length..].Trim();
            /* Validate against firebase */
            var uid = await service.ValidateFirebaseToken(token);
            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized("Token is Invalid");
            }
            /* Fetch user associated with token */
            var currentUser = await Util.UserHandler.UserHandler.GetUser(uid, context);
            /* validate answer, and return response as json. */
            var validation = mode switch
            {
                "character" => await Util.RelationHandler.UserCharacterRelationHandler.ValidateAnswer(currentUser, context, answer, id),
                "phrase" => await Util.RelationHandler.UserWordRelationshipHandler.ValidateAnswer(currentUser, context, answer, id),
                _ => throw new Exception($"current mode not supported, {mode}")
            };
            
            return Ok(validation);
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
    public async Task<IActionResult> GetUserInfo([FromQuery] string mode)
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
            var uid = await service.ValidateFirebaseToken(token);
            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized("Token is Invalid");
            }
            /* Fetch user associated with token */
            var currentUser = await Util.UserHandler.UserHandler.GetUser(uid, context);
            var userStats = mode switch
            {
                "character" => await Util.UserHandler.UserHandler.GetCharacterStats(currentUser, context),
                "phrase" => await Util.UserHandler.UserHandler.GetPhraseStats(currentUser, context),
                _ => throw new Exception($"current mode not supported, {mode}")
            };
            
            return Ok(userStats);
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