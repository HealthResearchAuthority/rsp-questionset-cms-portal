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
    IPublishedContentQuery contentService,
    IPublishedValueFallback publishedValueFallback) : ControllerBase
{
    [HttpGet("relatedQuestions")]
    public async Task<List<string>> GetRelatedQuestions(string questionId, string answerId)
    {
        var result = new List<string>();

        var currentQuestionSet = GetQuestionsetByVersion("modifications");

        var currentQuestion = currentQuestionSet?
            .Descendants<QuestionSlot>()
            .FirstOrDefault(x => x.QuestionId == questionId);

        if (currentQuestionSet == null || currentQuestion == null)
        {
            return result;
        }

        // Get all related questions first
        var relatedQuestionIds = GetRelatedQuestionIds(currentQuestion.Id, currentQuestionSet);

        if (!relatedQuestionIds.Any())
            return result;

        var questions = currentQuestionSet.Descendants<QuestionSlot>()
            .Where(q => relatedQuestionIds.Contains(q.QuestionId))
            .ToList();

        foreach (var question in questions)
        {
            var validationRules = question.ValidationRules?
                .Select(x => x.Content)
                .OfType<ValidationRule>();

            var hasMatchingAnswerCondition = validationRules?.Any(rule =>
                rule.Conditions?
                    .Select(c => c.Content)
                    .OfType<ValidationCondition>()
                    .Any(condition =>
                        condition.ParentOptions != null &&
                        condition.ParentOptions
                            .Select(p => new AnswerOption(p, publishedValueFallback))
                            .Any(opt => opt.OptionId == answerId)
                    ) == true
            ) == true;

            // Only include questions NOT related to this answerId
            if (!hasMatchingAnswerCondition)
            {
                result.Add(question.QuestionId);
            }
        }

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