using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;

namespace Kanji_teacher_backend.Util;

public class UserCharacterRelationHandler
{
    /// <summary>
    /// Function to create a relation between a user and their current max-grade characters.
    /// </summary>
    /// <param name="user"> the current User object from the database.</param>
    /// <param name="context"> the database context to find all valid characters. </param>
    public static void CreateRelation(UserTable user, KTContext context)
    {
        List<UserCharacterRelation> newRelations = [];
        var Chars = context.Characters.Where(e => e.JLPT == user.MaxGrade).ToList();
        if (Chars.Count > 0)
        {
            for (int i = 0; i < Chars.Count; i++)
            {
                UserCharacterRelation relation = new()
                {
                    User = user,
                    Char = Chars[i],
                    TimesAttempted = 0,
                    TimesCompleted = 0
                };
                newRelations.Add(relation);
            }
            context.UserCharacterRelations.AddRange(newRelations);
        }
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
    public static object GetRelationAndAnswers(UserTable? user, KTContext context)
    {
        if (user != null)
        {
            var selectRelation = context.UserCharacterRelations.Where(e => e.User == user)
                                                            .Include(e => e.Char)
                                                            .OrderBy(e => EF.Functions.Random() / (1.0 / (e.Char.Freq + e.TimesCompleted)))
                                                            .AsNoTracking()
                                                            .FirstOrDefault()
                                                            ?? throw new NullReferenceException($"could not find a relation");
            var relId = selectRelation.Id;
            var relAnswer = selectRelation.Char.Description;
            var relKanji = selectRelation.Char.Char;
            var OnReadings = string.Join(", ", selectRelation.Char.OnReadings.Split(", ").Take(3));
            var KunReadings = string.Join(", ", selectRelation.Char.KunReadings.Split(", ").Take(3));
            var OnRomanji = string.Join(", ", selectRelation.Char.OnRomanji.Split(", ").Take(3));
            var KunRomanji = string.Join(", ", selectRelation.Char.KunRomanji.Split(", ").Take(3));
            var answerList = context.Characters.Where(e => e.Description != relAnswer && e.JLPT == selectRelation.Char.JLPT)
                                                .OrderBy(e => EF.Functions.Random())
                                                .Select(e => e.Description)
                                                .AsNoTracking()
                                                .Take(3)
                                                .ToList();
            answerList.Add(relAnswer);
            return new
            {
                Id = relId,
                Alternatives = answerList,
                Kanji = relKanji,
                OnReadings = OnReadings == "" ? OnReadings : OnReadings + " | " + OnRomanji,
                KunReadings = KunReadings == "" ? KunReadings : KunReadings + " | " + KunRomanji
            };
        }
        else
        {
            var randomChar = context.Characters.Where(e => e.JLPT == 4)
                                        .OrderBy(e => EF.Functions.Random())
                                        .AsNoTracking()
                                        .FirstOrDefault()
                                        ?? throw new NullReferenceException($"could not find a random character");
            var answerList = context.Characters.Where(e => e.Description != randomChar.Description && e.Grade == randomChar.Grade)
                                                .OrderBy(e => EF.Functions.Random())
                                                .Select(e => e.Description)
                                                .AsNoTracking()
                                                .Take(3)
                                                .ToList();
            answerList.Add(randomChar.Description);
            var OnReadings = string.Join(", ", randomChar.OnReadings.Split(", ").Take(3));
            var KunReadings = string.Join(", ", randomChar.KunReadings.Split(", ").Take(3));
            var OnRomanji = string.Join(", ", randomChar.OnRomanji.Split(", ").Take(3));
            var KunRomanji = string.Join(", ", randomChar.KunRomanji.Split(", ").Take(3));
            return new
            {
                randomChar.Id,
                Alternatives = answerList,
                Kanji = randomChar.Char,
                OnReadings = OnReadings == "" ? OnReadings : OnReadings + " | " + OnRomanji,
                KunReadings = KunReadings == "" ? KunReadings : KunReadings + " | " + KunRomanji
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
    public static object ValidateAnswer(UserTable? user, KTContext context, string answer, int id)
    {
        if (user != null)
        {
            var correctRelation = context.UserCharacterRelations.Include(e => e.Char)
                                                           .Include(e => e.User)
                                                           .FirstOrDefault(e => e.Id == id) ?? throw new NullReferenceException($"Database missmatch, no relation with Id {id}");
            correctRelation.TimesAttempted += 1;
            var correctAnswer = correctRelation.Char.Description;
            if (correctAnswer == answer)
            {
                correctRelation.TimesCompleted += 1;
                user.Xp += 2;
                context.SaveChanges();
                return new
                {
                    CharacterInfo = new
                    {
                        Grade = correctRelation.Char.JLPT,
                        Description = correctAnswer,
                        Char = correctRelation.Char.Char,
                    },
                    Correct = true,
                    CanProgress = ProgressHandler.CheckProgress(user)
                };
            }
            else
            {
                if (user.Xp > 1) user.Xp -= 1;
                else user.Xp = 0;
                context.SaveChanges();
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
        }
        else
        {
            var correctChar = context.Characters.FirstOrDefault(e => e.Id == id) ?? throw new NullReferenceException($"Database missmatch, no character with Id {id}");
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