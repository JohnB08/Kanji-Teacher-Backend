using Kanji_teacher_backend.dbContext;
using Kanji_teacher_backend.models;
using Microsoft.EntityFrameworkCore;

namespace Kanji_teacher_backend.Util;

public class RelationHandler
{
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
    public static object ValidateAnswer(UserTable user, KTContext context, string answer, int id)
    {
        var correctRelation = context.UserCharacterRelations.Find(id) ?? throw new NullReferenceException($"Database missmatch, no relation with Id {id}");
        if (correctRelation.User != user) throw new Exception($"User missmatch, {id} not a valid question ID.");
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