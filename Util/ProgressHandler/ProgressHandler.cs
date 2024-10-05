using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;

namespace Kanji_teacher_backend.Util;

public class ProgressHandler
{
    public static bool CheckProgress(UserTable user, KTContext context)
    {
        var Grade = user.MaxGrade;
        var Attemts = context.UserCharacterRelations.Where(e => e.User == user).Sum(e => e.TimesAttempted);
        var Completed = context.UserCharacterRelations.Where(e => e.User == user).Sum(e => e.TimesCompleted);
        return Grade switch
        {
            1 => Completed > 50 && (float)Completed / (float)Attemts > 0.50,
            2 => Completed > 100 && (float)Completed / (float)Attemts > 0.60,
            3 => Completed > 150 && (float)Completed / (float)Attemts > 0.70,
            4 => Completed > 200 && (float)Completed / (float)Attemts > 0.80,
            5 => Completed > 250 && (float)Completed / (float)Attemts > 0.80,
            6 => Completed > 300 && (float)Completed / (float)Attemts > 0.80,
            7 => Completed > 350 && (float)Completed / (float)Attemts > 0.80,
            8 => Completed > 200 && (float)Completed / (float)Attemts > 0.80,
            _ => false,
        };
    }
    public static void UpgradeGrade(UserTable user, KTContext context)
    {
        if (CheckProgress(user, context))
        {
            user.MaxGrade += 1;
            context.SaveChanges();
            RelationHandler.CreateRelation(user, context);
            return;
        }
        return;
    }

}