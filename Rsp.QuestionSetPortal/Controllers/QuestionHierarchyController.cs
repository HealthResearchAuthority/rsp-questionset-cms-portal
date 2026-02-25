using Microsoft.AspNetCore.Mvc;
using Rsp.QuestionSetPortal.DTOs.Requests;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
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
    public async Task<List<string>> GetRelatedQuestions([FromQuery] RelatedQuestionsRequest request)
    {
        var result = new List<string>();

        var currentQuestionSet = GetQuestionsetByVersion("modifications");

        var currentQuestion = currentQuestionSet?
            .Descendants<QuestionSlot>()
            .FirstOrDefault(x => x.QuestionId == request.QuestionId);

        if (currentQuestionSet == null || currentQuestion == null)
        {
            return result;
        }

        // Normalise for safety + performance
        var selectedAnswerSet = request.AnswerIds?
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries))
        .Select(x => x.Trim())
        .ToHashSet(StringComparer.OrdinalIgnoreCase)
        ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!selectedAnswerSet.Any())
            return result;

        // Get all related questions first
        var relatedQuestionIds = GetRelatedQuestionIds(currentQuestion.Id, currentQuestionSet);

        if (!relatedQuestionIds.Any())
            return result;

        var questions = currentQuestionSet.Descendants<QuestionSlot>()
            .Where(q => !string.IsNullOrEmpty(q.QuestionId) && relatedQuestionIds.Contains(q.QuestionId!))
            .ToList();

        var triggeredQuestionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var question in questions)
        {
            var validationRules = question.ValidationRules?
                .Select(x => x.Content)
                .OfType<ValidationRule>();

            var hasMatchingAnswerCondition = validationRules?
            .SelectMany(rule => rule.Conditions ?? Enumerable.Empty<BlockListItem>())
            .Select(c => c.Content)
            .OfType<ValidationCondition>()
            .Any(condition =>
            {
                if (condition.ParentOptions == null || !condition.ParentOptions.Any())
                    return false;

                var parentOptionIds = condition.ParentOptions
                    .Select(p => new AnswerOption(p, publishedValueFallback))
                    .Select(opt => opt.OptionId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // triggered if ANY selected answer matches this condition
                return selectedAnswerSet.Any(answerId => parentOptionIds.Contains(answerId));
            }) == true;

            if (!hasMatchingAnswerCondition && !string.IsNullOrEmpty(question!.QuestionId))
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