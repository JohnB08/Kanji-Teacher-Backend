using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Kanji_teacher_backend.Util;

public class UserWordRelationshipHandler
{
    /// <summary>
    /// Function to create a relation between a user and their current max-grade characters.
    /// </summary>
    /// <param name="user"> the current User object from the database.</param>
    /// <param name="context"> the database context to find all valid characters. </param>
    public static void CreateRelation(UserTable user, KTContext context)
    {
        List<UserWordRelation> newRelations = [];
        var Words = context.Words.Where(e => e.JLPT == user.MaxGrade).ToList();
        if (Words.Count > 0)
        {
            for (int i = 0; i < Words.Count; i++)
            {
                UserWordRelation relation = new()
                {
                    User = user,
                    Word = Words[i],
                    TimesAttempted = 0,
                    TimesCompleted = 0
                };
                newRelations.Add(relation);
            }
            context.UserWordRelations.AddRange(newRelations);
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
    public static object GetRelationAndAnswers(UserTable user, KTContext context)
    {
        {
            var selectRelation = context.UserWordRelations.Where(e => e.User == user)
                                                            .Include(e => e.Word)
                                                            .OrderBy(e => EF.Functions.Random() / (e.Word.Weight + (1.0 / (e.TimesCompleted + 1))))
                                                            .AsNoTracking()
                                                            .FirstOrDefault()
                                                            ?? throw new NullReferenceException($"could not find a relation");
            var relId = selectRelation.Id;
            var relAnswer = selectRelation.Word.Description.Split(", ").FirstOrDefault();
            var relKanji = selectRelation.Word.Written;
            var Pronounciation = selectRelation.Word.Pronounciation;
            var Romanji = selectRelation.Word.Romanji;
            var answerList = context.WordCharacterRelations
                                    .Where(wcr => wcr.WordId == selectRelation.CharId)
                                    .SelectMany(wcr => context.WordCharacterRelations
                                                .Include(rel => rel.Word)
                                                .Where(rel => rel.CharId == wcr.CharId && rel.Word.JLPT == selectRelation.Word.JLPT)
                                                .Select(rel => rel.Word.Description))
                                    .OrderBy(d => EF.Functions.Random())
                                    .Take(3)
                                    .AsNoTracking()
                                    .ToList();
            answerList.Add(relAnswer);
            return new
            {
                Id = relId,
                Alternatives = answerList,
                Kanji = relKanji,
                OnReadings = Pronounciation == "" ? Pronounciation : Pronounciation + " | " + Romanji,
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
    public static object ValidateAnswer(UserTable user, KTContext context, string answer, int id)
    {
        var correctRelation = context.UserWordRelations.Include(e => e.Word)
                                                       .Include(e => e.User)
                                                       .FirstOrDefault(e => e.Id == id) ?? throw new NullReferenceException($"Database missmatch, no relation with Id {id}");
        correctRelation.TimesAttempted += 1;
        var correctAnswer = correctRelation.Word.Description.Split(", ").FirstOrDefault();
        if (correctAnswer == answer)
        {
            correctRelation.TimesCompleted += 1;
            user.Xp += 2;
            context.SaveChanges();
            return new
            {
                CharacterInfo = new
                {
                    Grade = correctRelation.Word.JLPT,
                    Description = correctAnswer,
                    Char = correctRelation.Word.Written,
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
                    Grade = correctRelation.Word.JLPT,
                    Description = correctAnswer,
                    Char = correctRelation.Word.Written,
                },
                Correct = false,
                CanProgress = false
            };
        }
    }
}