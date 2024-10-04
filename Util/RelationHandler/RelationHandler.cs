using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;
using Microsoft.EntityFrameworkCore;

namespace Kanji_teacher_backend.Util;

public class RelationHandler
{
    /// <summary>
    /// Function to create a relation between a user and their current max-grade characters.
    /// </summary>
    /// <param name="user"> the current User object from the database.</param>
    /// <param name="context"> the database context to find all valid characters. </param>
    public static void CreateRelation(UserTable user, KTContext context)
    {
        List<UserCharacterRelation> newRelations = [];
        var Chars = context.Characters.Where(e => e.Grade == user.MaxGrade).ToList();
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
        var selectRelation = context.UserCharacterRelations.Where(e => e.User == user)
                                                            .OrderBy(e => EF.Functions.Random())
                                                            .First()
                                                             ??
                                                            throw new NullReferenceException($"Database relation missmatch. Missing relation for user {user.Id}");
        var relId = selectRelation.Id;
        var relAnswer = selectRelation.Char.Description;
        var relKanji = selectRelation.Char.Char;
        var answerList = context.Characters.Where(e => e.Description != relAnswer)
                                            .OrderBy(e => EF.Functions.Random())
                                            .Select(e => e.Description)
                                            .ToList();
        answerList.Add(relAnswer);
        return new
        {
            Id = relId,
            Alternatives = answerList,
            Kanji = relKanji
        };
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
        var correctRelation = context.UserCharacterRelations.Find(id) ?? throw new NullReferenceException($"Database missmatch, no relation with Id {id}");
        if (correctRelation.User != user) throw new Exception($"User missmatch, {user.Id} is not associated with {correctRelation.Id}");
        correctRelation.TimesAttempted += 1;
        if (correctRelation.Char.Description == answer)
        {
            correctRelation.TimesCompleted += 1;
            context.SaveChanges();
            return new
            {
                CharacterInfo = correctRelation.Char,
                Correct = true
            };
        }
        else
        {
            context.SaveChanges();
            return new
            {
                CharaterInfo = correctRelation.Char,
                Correct = false
            };
        }
    }
}