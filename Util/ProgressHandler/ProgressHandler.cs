using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;

namespace Kanji_teacher_backend.Util.ProgressHandler;

public static class ProgressHandler
{
    public static bool CheckProgress(UserTable user)
    {
        var grade = user.MaxGrade;
        if (grade < 1) return false;
        var xp = user.Xp;
        return xp > 100 * (10 - grade);
    }
    public static async Task UpgradeGrade(UserTable user, KtContext context)
    {
        if (!CheckProgress(user)) return;
        {
            user.MaxGrade -= 1;
            user.Xp = 0;
            await RelationHandler.UserCharacterRelationHandler.CreateRelation(user, context);
            await RelationHandler.UserWordRelationshipHandler.CreateRelation(user, context);
            await context.SaveChangesAsync();
        }
    }

}