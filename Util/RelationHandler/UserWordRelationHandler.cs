using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;
using Microsoft.EntityFrameworkCore;

namespace Kanji_teacher_backend.Util.RelationHandler;

public static class UserWordRelationshipHandler
{
    /// <summary>
    /// Function to create a relation between a user and their current max-grade characters.
    /// </summary>
    /// <param name="user"> the current User object from the database.</param>
    /// <param name="context"> the database context to find all valid characters. </param>
    public static async Task CreateRelation(UserTable user, KtContext context)
    {
        List<UserWordRelation> newRelations = [];
        var words = context.Words.Where(e => e.JLPT == user.MaxGrade).ToList();
        if (words.Count <= 0) return;
        newRelations.AddRange(words.Select(t => new UserWordRelation() { User = user, Word = t, TimesAttempted = 0, TimesCompleted = 0 }));
        await context.UserWordRelations.AddRangeAsync(newRelations);
    }
    /// <summary>
    /// Function to fetch a random relation based on a user. and generates data for a flash card on the front end. 
    /// </summary>
    /// <param name="user">the current user object from the database</param>
    /// <param name="context">the database context. </param>
    /// <returns>
    /// {
    ///     Id: int (id of selected relation)
    ///     Kanji: string (the chosen character)
    ///     Alternatives: [string] (A list of four alternatives used to generate buttons at the front end).
    /// }
    /// </returns>
    /// <exception cref="NullReferenceException"> If it can't find a valid relationship on the current user, it throws a nullreference exception. </exception>
    public static async Task<object> GetRelationAndAnswers(UserTable user, KtContext context)
    {
        {
            var selectRelation = await context.UserWordRelations.Where(e => e.User == user)
                                                            .Include(e => e.Word)
                                                            .OrderBy(e => EF.Functions.Random() / e.Word.Weight * (e.TimesCompleted + 1))
                                                            .AsNoTracking()
                                                            .FirstOrDefaultAsync()
                                                            ?? throw new NullReferenceException($"could not find a relation");
            var relId = selectRelation.Id;
            var relAnswer = selectRelation.Word.Description.Split(", ").FirstOrDefault() ?? "";
            var relKanji = selectRelation.Word.Written;
            var pronounciation = selectRelation.Word.Pronounciation;
            var romanji = selectRelation.Word.Romanji;
            var answerList = await context.WordCharacterRelations
                                    .Where(wcr => wcr.WordId == selectRelation.CharId)
                                    .SelectMany(wcr => context.WordCharacterRelations
                                                .Include(rel => rel.Word)
                                                .Where(rel => rel.CharId == wcr.CharId && rel.Word.JLPT == selectRelation.Word.JLPT)
                                                .Select(rel => rel.Word.Description))
                                    .OrderBy(d => EF.Functions.Random())
                                    .Take(3)
                                    .AsNoTracking()
                                    .ToListAsync();
            answerList.Add(relAnswer);
            return new
            {
                Id = relId,
                Alternatives = answerList,
                Kanji = relKanji,
                OnReadings = pronounciation == "" ? pronounciation : pronounciation + " | " + romanji,
                KunReadings = ""
            };
        }
    }
    /// <summary>
    /// Validates a submitted answer against the relation ID. 
    /// Returns the data for the character in the relation, aswell as a True or False.
    /// Updates the relation Attempted and Completed counts deppending on if the answer is correct or not. 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="context"></param>
    /// <param name="answer"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">If it cannot find a relation based on the ID</exception>
    /// <exception cref="Exception">If somehow the user is not connected to the relation with the id of Param.id, throw an exception.</exception>
    public static async Task<object> ValidateAnswer(UserTable user, KtContext context, string answer, int id)
    {
        var correctRelation = await context.UserWordRelations.Include(e => e.Word)
                                                       .Include(e => e.User)
                                                       .FirstOrDefaultAsync(e => e.Id == id) ?? throw new NullReferenceException($"Database missmatch, no relation with Id {id}");
        correctRelation.TimesAttempted += 1;
        var correctAnswer = correctRelation.Word.Description.Split(", ").FirstOrDefault();
        if (correctAnswer == answer)
        {
            correctRelation.TimesCompleted += 1;
            user.Xp += 2;
            await context.SaveChangesAsync();
            return new
            {
                CharacterInfo = new
                {
                    Grade = correctRelation.Word.JLPT,
                    Description = correctAnswer,
                    Char = correctRelation.Word.Written,
                },
                Correct = true,
                CanProgress = ProgressHandler.ProgressHandler.CheckProgress(user)
            };
        }
        if (user.Xp > 1) user.Xp -= 1;
            else user.Xp = 0;
        await context.SaveChangesAsync();
        return new
            {
                CharacterInfo = new
                {
                    Grade = correctRelation.Word.JLPT,
                    Description = correctAnswer,
                    Char = correctRelation.Word.Written,
                },
                Correct = false,
                CanProgress = false
            };
    }
}