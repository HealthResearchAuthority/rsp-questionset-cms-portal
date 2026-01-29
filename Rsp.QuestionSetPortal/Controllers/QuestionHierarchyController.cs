using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Controllers;

[ApiController]
[Route("/umbraco/api/questionHierarchy")]
public class QuestionHierarchyController(
    IUmbracoContextAccessor contentQuery,
    IPublishedContentQuery contentService) : ControllerBase
{
    [HttpGet("relatedQuestions")]
    public async Task<List<string>> GetRelatedQuestions(string questionId, string answerId)
    {
        var result = new List<string>();

        var currentQuestionSet = GetQuestionsetByVersion("modifications");

        var currentQuestion = currentQuestionSet?.Descendants<QuestionSlot>().FirstOrDefault(x => x.QuestionId == questionId);

        if (currentQuestionSet == null || currentQuestion == null)
        {
            return new List<string>();
        }

        result.AddRange(GetRelatedQuestionIds(currentQuestion.Id, currentQuestionSet));

        return result;
    }

    private List<string> GetRelatedQuestionIds(
    int questionId,
    IPublishedContent questionSet,
    HashSet<int>? visited = null)
    {
        visited ??= new HashSet<int>();

        if (!visited.Add(questionId))
            return new List<string>(); // already processed

        var result = new List<string>();

        var questions = questionSet.Descendants<QuestionSlot>();

        foreach (var question in questions)
        {
            var validationRules = question.ValidationRules?
                .Select(x => x.Content)
                .OfType<ValidationRule>();

            var questionContent = question.QuestionContent as Question;
            var answers = questionContent.PossibleAnswers as IEnumerable<AnswerOption>;

            foreach (var a in answers)
            {
                var ID = a.OptionId;
            }


            if (validationRules?.Any(x => x.ParentQuestion != null && x.ParentQuestion.Id == questionId) == true)
            {
                result.Add(question.QuestionId);

                result.AddRange(
                    GetRelatedQuestionIds(question.Id, questionSet, visited)
                );
            }
        }

        return result;
    }

    //private List<string> GetRelatedQuestionIds(int questionId, QuestionSet questionSet)
    //{
    //    var result = new List<string>();
    //    // now loop over all questions in the question set to find
    //    // those that have a condition matching the current question and answer
    //    var questions = questionSet.Descendants<QuestionSlot>();

    //    foreach (var question in questions)
    //    {
    //        var validationRules = question.ValidationRules?.Select(x => x.Content) as IEnumerable<ValidationRule>;

    //        if (validationRules != null)
    //        {
    //            // check if this question has any validation rules that relate to the original question
    //            var matchingRule = validationRules.Any(x => x.ParentQuestion.Id == questionId);
    //            if (matchingRule)
    //            {
    //                result.Add(question.QuestionId);
    //            }
    //        }
    //        result.AddRange(GetRelatedQuestionIds(question.Id, questionSet));
    //    }

    //    return result;
    //}

    private IPublishedContent? GetQuestionsetByVersion(string type = "projectRecord")
    {
        IPublishedContent? questionsetRepo = null;
        if (type == "projectRecord")
        {
            questionsetRepo = contentService.ContentAtRoot()?
            .FirstOrDefault(x => x.ContentType.Alias == Questionsets.ModelTypeAlias)?
            .Descendant<QuestionsetRepository>();
        }
        else
        {
            questionsetRepo = contentService.ContentAtRoot()?
            .FirstOrDefault(x => x.ContentType.Alias == Questionsets.ModelTypeAlias)?
            .Descendant<ModificationsQuestionsetRepository>();
        }

        if (questionsetRepo != null)
        {
            // get active questionset because version is not specified
            var activeQuestionSet = questionsetRepo.Value<IPublishedContent>("activeQuestionset");
            return activeQuestionSet;
        }

        // questionset repository does not exist
        return null;
    }
}