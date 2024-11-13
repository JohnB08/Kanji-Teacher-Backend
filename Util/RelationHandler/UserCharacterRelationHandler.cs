using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;
using Microsoft.EntityFrameworkCore;


namespace Kanji_teacher_backend.Util.RelationHandler;

public class UserCharacterRelationHandler
{
    /// <summary>
    /// Function to create a relation between a user and their current max-grade characters.
    /// </summary>
    /// <param name="user"> the current User object from the database.</param>
    /// <param name="context"> the database context to find all valid characters. </param>
    public static async Task CreateRelation(UserTable user, KtContext context)
    {
        List<UserCharacterRelation> newRelations = [];
        var chars = await context.Characters.Where(e => e.JLPT == user.MaxGrade).ToListAsync();
        if (chars.Count <= 0) return;
        newRelations.AddRange(chars.Select(t => new UserCharacterRelation() { User = user, Char = t, TimesAttempted = 0, TimesCompleted = 0 }));
        await context.UserCharacterRelations.AddRangeAsync(newRelations);
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
    public static async Task<object> GetRelationAndAnswers(UserTable? user, KtContext context)
    {
        if (user != null)
        {
            var selectRelation = await context
                                     .UserCharacterRelations.Where(e => e.User == user)
                                     .Include(e => e.Char)
                                     .OrderBy(e => EF.Functions.Random() / (1.0 / (e.Char.Freq + e.TimesCompleted)))
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync()
                                 ?? throw new NullReferenceException("could not find a relation");
            var relId = selectRelation.Id;
            var relAnswer = selectRelation.Char.Description;
            var relKanji = selectRelation.Char.Char;
            var onReadings = string.Join(", ", selectRelation.Char.OnReadings.Split(", ").Take(3));
            var kunReadings = string.Join(", ", selectRelation.Char.KunReadings.Split(", ").Take(3));
            var onRomanji = string.Join(", ", selectRelation.Char.OnRomanji.Split(", ").Take(3));
            var kunRomanji = string.Join(", ", selectRelation.Char.KunRomanji.Split(", ").Take(3));
            var answerList = await context.Characters
                .Where(e => e.Description != relAnswer && e.JLPT == selectRelation.Char.JLPT)
                .OrderBy(e => EF.Functions.Random())
                .Select(e => e.Description)
                .AsNoTracking()
                .Take(3)
                .ToListAsync();
            
            answerList.Add(relAnswer);
            return new
            {
                Id = relId,
                Alternatives = answerList,
                Kanji = relKanji,
                OnReadings = onReadings == "" ? onReadings : onReadings + " | " + onRomanji,
                KunReadings = kunReadings == "" ? kunReadings : kunReadings + " | " + kunRomanji
            };
        }
        else
        {
            var randomChar = await context.Characters.Where(e => e.JLPT == 4)
                                        .OrderBy(e => EF.Functions.Random())
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync()
                                        ?? throw new NullReferenceException($"could not find a random character");
            var answerList = await context.Characters.Where(e => e.Description != randomChar.Description && e.Grade == randomChar.Grade)
                                                .OrderBy(e => EF.Functions.Random())
                                                .Select(e => e.Description)
                                                .AsNoTracking()
                                                .Take(3)
                                                .ToListAsync();
            answerList.Add(randomChar.Description);
            var onReadings = string.Join(", ", randomChar.OnReadings.Split(", ").Take(3));
            var kunReadings = string.Join(", ", randomChar.KunReadings.Split(", ").Take(3));
            var onRomanji = string.Join(", ", randomChar.OnRomanji.Split(", ").Take(3));
            var kunRomanji = string.Join(", ", randomChar.KunRomanji.Split(", ").Take(3));
            return new
            {
                randomChar.Id,
                Alternatives = answerList,
                Kanji = randomChar.Char,
                OnReadings = onReadings == "" ? onReadings : onReadings + " | " + onRomanji,
                KunReadings = kunReadings == "" ? kunReadings : kunReadings + " | " + kunRomanji
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
    public static async Task<object> ValidateAnswer(UserTable? user, KtContext context, string answer, int id)
    {
        if (user != null)
        {
            var correctRelation = await context.UserCharacterRelations
                .Include(e => e.Char)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id) ?? throw new NullReferenceException($"Database missmatch, no relation with Id {id}");
            correctRelation.TimesAttempted += 1;
            var correctAnswer = correctRelation.Char.Description;
            if (correctAnswer == answer)
            {
                correctRelation.TimesCompleted += 1;
                user.Xp += 2;
                await context.SaveChangesAsync();
                return new
                {
                    CharacterInfo = new
                    {
                        Grade = correctRelation.Char.JLPT,
                        Description = correctAnswer,
                        Char = correctRelation.Char.Char,
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
                        Grade = correctRelation.Char.JLPT,
                        Description = correctAnswer,
                        Char = correctRelation.Char.Char,
                    },
                    Correct = false,
                    CanProgress = false
                };
        }
        else
        {
            var correctChar = await context.Characters.FirstOrDefaultAsync(e => e.Id == id) ?? throw new NullReferenceException($"Database missmatch, no character with Id {id}");
            var correctAnswer = correctChar.Description.Split(", ").FirstOrDefault();
            return new
            {
                CharacterInfo = new
                {
                    Grade = correctChar.JLPT,
                    Description = correctAnswer,
                    Char = correctChar.Char,
                },
                Correct = correctAnswer == answer,
                CanProgress = false
            };

        }
    }
}