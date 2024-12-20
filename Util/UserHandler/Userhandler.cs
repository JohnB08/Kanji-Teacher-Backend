using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;
using Microsoft.EntityFrameworkCore;

namespace Kanji_teacher_backend.Util.UserHandler;

public static class UserHandler
{
    /// <summary>
    /// Create a new user in the database based on the uid. 
    /// Also creates a relation between the user and all valid characters. 
    /// </summary>
    /// <param name="uid">the uid generated by firebase</param>
    /// <param name="context">the database context</param>
    /// <returns>user object</returns>
    private static async Task<UserTable> CreateUser(string uid, KtContext context)
    {
        UserTable user = new()
        {
            Uid = uid,
            MaxGrade = 5,
            Xp = 0
        };
        context.Users.Add(user);
        await RelationHandler.UserCharacterRelationHandler.CreateRelation(user, context);
        await RelationHandler.UserWordRelationshipHandler.CreateRelation(user, context);
        await context.SaveChangesAsync();
        return user;
    }
    /// <summary>
    /// Gets the user with input uid. If no user is found it creates a new user. 
    /// </summary>
    /// <param name="uid">the uid generated by firebase</param>
    /// <param name="context">the database context</param>
    /// <returns>user object</returns>
    public static async Task<UserTable> GetUser(string uid, KtContext context)
    {
        var existingUser = await context.Users.FirstOrDefaultAsync(e => e.Uid == uid) ?? await CreateUser(uid, context);
        return existingUser;
    }
    public static async Task<object> GetCharacterStats(UserTable user, KtContext context)
    {
        var wordData = await context.UserCharacterRelations
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
                                .FirstOrDefaultAsync() 
                                ?? throw new NullReferenceException($"Missing data for user, {user.Id}");
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
    public static async Task<object> GetPhraseStats(UserTable user, KtContext context)
    {
        var wordData = await context.UserWordRelations
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
                                .FirstOrDefaultAsync() 
                                ?? throw new NullReferenceException($"missing data for user, {user.Id}");
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