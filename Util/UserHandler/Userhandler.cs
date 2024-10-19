using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;
using Microsoft.EntityFrameworkCore;

namespace Kanji_teacher_backend.Util;

public class UserHandler
{
    /// <summary>
    /// Create a new user in the database based on the uid. 
    /// Also creates a relation between the user and all valid characters. 
    /// </summary>
    /// <param name="uid">the uid generated by firebase</param>
    /// <param name="context">the database context</param>
    /// <returns>user object</returns>
    private static UserTable CreateUser(string uid, KTContext context)
    {
        UserTable user = new()
        {
            Uid = uid,
            MaxGrade = 5,
            Xp = 0
        };
        context.Users.Add(user);
        UserCharacterRelationHandler.CreateRelation(user, context);
        UserWordRelationshipHandler.CreateRelation(user, context);
        context.SaveChanges();
        return user;
    }
    /// <summary>
    /// Gets the user with input uid. If no user is found it creates a new user. 
    /// </summary>
    /// <param name="uid">the uid generated by firebase</param>
    /// <param name="context">the database context</param>
    /// <returns>user object</returns>
    public static UserTable GetUser(string uid, KTContext context)
    {
        var existingUser = context.Users.FirstOrDefault(e => e.Uid == uid) ?? CreateUser(uid, context);
        return existingUser;
    }
    public static object GetCharacterStats(UserTable user, KTContext context)
    {
        var wordData = context.UserCharacterRelations
                                .Where(e => e.User == user)
                                .Include(e => e.Char)
                                .GroupBy(e => e.User)
                                .Select(
                                    g => new
                                    {
                                        TimesCompleted = g.Sum(e => e.TimesCompleted),
                                        TimesAttempted = g.Sum(e => e.TimesAttempted),
                                        MostCompletedChar = g.OrderByDescending(e => e.TimesCompleted).FirstOrDefault(),
                                        MostAttemptedChar = g.OrderByDescending(e => e.TimesAttempted).FirstOrDefault()
                                    }
                                )
                                .AsNoTracking()
                                .FirstOrDefault();
        if (wordData == null) throw new NullReferenceException($"Missing data for user, {user.Id}");
        return new
        {
            Grade = user.MaxGrade,
            wordData.TimesCompleted,
            wordData.TimesAttempted,
            CurrentProgress = user.Xp,
            CurrentLimit = (6 - user.MaxGrade) * 100,
            MostCompleted = new
            {
                wordData.MostCompletedChar?.Char.Description,
                wordData.MostCompletedChar?.Char.Char,
                Attempted = wordData.MostCompletedChar?.TimesAttempted,
                Completed = wordData.MostCompletedChar?.TimesCompleted
            },
            MostAttempted = new
            {
                wordData.MostAttemptedChar?.Char.Description,
                wordData.MostAttemptedChar?.Char.Char,
                Attempted = wordData.MostAttemptedChar?.TimesAttempted,
                Completed = wordData.MostAttemptedChar?.TimesCompleted
            },
            SuccessRate = $"{(float)wordData.TimesCompleted / wordData.TimesAttempted:P2}"
        };
    }
    public static object GetPhraseStats(UserTable user, KTContext context)
    {
        var wordData = context.UserWordRelations
                                .Where(e => e.User == user)
                                .Include(e => e.Word)
                                .GroupBy(e => e.User)
                                .Select(
                                    g => new
                                    {
                                        TimesCompleted = g.Sum(e => e.TimesCompleted),
                                        TimesAttempted = g.Sum(e => e.TimesAttempted),
                                        MostCompletedWord = g.OrderByDescending(e => e.TimesCompleted).FirstOrDefault(),
                                        MostAttemptedWord = g.OrderByDescending(e => e.TimesAttempted).FirstOrDefault()
                                    }
                                )
                                .FirstOrDefault();
        if (wordData == null) throw new NullReferenceException($"missing data for user, {user.Id}");
        return new
        {
            Grade = user.MaxGrade,
            wordData.TimesCompleted,
            wordData.TimesAttempted,
            CurrentProgress = user.Xp,
            CurrentLimit = (6 - user.MaxGrade) * 100,
            MostCompleted = new
            {
                wordData.MostCompletedWord?.Word.Description,
                Char = wordData.MostAttemptedWord?.Word.Written,
                Attempted = wordData.MostAttemptedWord?.TimesAttempted,
                Completed = wordData.MostCompletedWord?.TimesCompleted
            },
            MostAttempted = new
            {
                wordData.MostCompletedWord?.Word.Description,
                Char = wordData.MostAttemptedWord?.Word.Written,
                Attempted = wordData.MostAttemptedWord?.TimesAttempted,
                Completed = wordData.MostAttemptedWord?.TimesCompleted
            },
            SuccessRate = $"{(float)wordData.TimesCompleted / wordData.TimesAttempted:P2}"
        };
    }
}