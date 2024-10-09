using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
namespace Kanji_teacher_backend.Controllers.SetupControllers;
[Route("api")]
[ApiController]
public class ConfigController : ControllerBase
{
    [HttpGet("config")]
    public IActionResult GetFirebaseConfig()
    {
        var firebaseConfig = new
        {
            apiKey = Environment.GetEnvironmentVariable("FIREBASE_APIKEY"),
            authDomain = Environment.GetEnvironmentVariable("FIREBASE_AUTHDOMAIN"),
            projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECTID"),
            storageBucket = Environment.GetEnvironmentVariable("FIREBASE_STORAGEBUCKET"),
            messagingSenderId = Environment.GetEnvironmentVariable("FIREBASE_MESSAGINGSENDERID"),
            appId = Environment.GetEnvironmentVariable("FIREBASE_APPID"),
        };
        var json = JsonSerializer.Serialize(firebaseConfig);
        return Ok(json);
    }
}