using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin.Auth;

namespace Kanji_teacher_backend.Util;

public class FirebaseService
{
    public FirebaseService()
    {
        FirebaseApp.Create(
            new AppOptions()
            {
                Credential = GoogleCredential.FromFile("./ConfigFiles/FirebaseConfig/FirebaseConfig.json")
            }
        );
    }
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
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}