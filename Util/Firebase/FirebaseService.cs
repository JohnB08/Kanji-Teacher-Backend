using System.Text;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Kanji_Teacher_Backend.Util.Firebase;

public class FirebaseService
{
    public FirebaseService()
    {
        var rawData = Environment.GetEnvironmentVariable("GOOGLE_AUTH_JSON") ?? throw new NullReferenceException("Missing google auth data.");
        if (string.IsNullOrEmpty(rawData))
        {
            throw new InvalidOperationException("Google credentials are not set in the environment variable.");
        }
        var data = Convert.FromBase64String(rawData);
        var googleCredentialsJson = Encoding.UTF8.GetString(data);
        FirebaseApp.Create(
            new AppOptions()
            {
                Credential = GoogleCredential.FromJson(googleCredentialsJson)
            }
        );
    }
    /// <summary>
    /// Function for validating a jwt token through Firebase.
    /// </summary>
    /// <param name="token">a string representing the JWT token.</param>
    /// <returns> the uid from the token, or throws a general error. </returns>
    public async Task<string> ValidateFirebaseToken(string token)
    {
        try
        {
            var firebaseToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
            var uid = firebaseToken.Uid;
            return uid;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}