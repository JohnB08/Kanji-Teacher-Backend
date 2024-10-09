using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin.Auth;
using SQLitePCL;
using System.Text;

namespace Kanji_teacher_backend.Util;

public class FirebaseService
{
    public FirebaseService()
    {
        string rawData = Environment.GetEnvironmentVariable("GOOGLE_AUTH_JSON") ?? throw new NullReferenceException("Missing google auth data.");
        if (string.IsNullOrEmpty(rawData))
        {
            throw new InvalidOperationException("Google credentials are not set in the environment variable.");
        }
        byte[] data = Convert.FromBase64String(rawData);
        string googleCredentialsJson = Encoding.UTF8.GetString(data);
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
            FirebaseToken firebaseToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
            string uid = firebaseToken.Uid;
            return uid;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}