using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;

namespace Kanji_teacher_backend.Util;

public class UserHandler
{
    private static UserTable CreateUser(string uid, KTContext context)
    {
        UserTable user = new()
        {
            Uid = uid,
            MaxGrade = 1
        };
        context.Users.Add(user);
        RelationHandler.CreateRelation(user, context);
        context.SaveChanges();
        return user;
    }
    public static UserTable GetUser(string uid, KTContext context)
    {
        var existingUser = context.Users.Where(e => e.Uid == uid).First();
        if (existingUser == null) return CreateUser(uid, context);
        else return existingUser;
    }
}