using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;

namespace Kanji_teacher_backend.Util;

public class ProgressHandler
{
    public static bool CheckProgress(UserTable user)
    {
        var Grade = user.MaxGrade;
        if (Grade < 1) return false;
        var xp = user.Xp;
        return xp > 100 * (6 - Grade);
    }
    public static void UpgradeGrade(UserTable user, KTContext context)
    {
        if (CheckProgress(user))
        {
            user.MaxGrade -= 1;
            user.Xp = 0;
            UserCharacterRelationHandler.CreateRelation(user, context);
            UserWordRelationshipHandler.CreateRelation(user, context);
            context.SaveChanges();
            return;
        }
        return;
    }

}