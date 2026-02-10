using Microsoft.AspNetCore.Mvc;
using Rsp.QuestionSetPortal.Models.QuestionSetMetaData;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Controllers;

[ApiController]
[Route("/umbraco/api/metadata")]
public class QuestionSetMetaDataApiController(
    IPublishedContentQuery contentQuery
    ) : ControllerBase
{
    [HttpGet("questionset")]
    [ProducesResponseType(typeof(List<QuestionSetMetaDataModel>), StatusCodes.Status200OK)]
    public IActionResult QuestionSetMetaData()
    {
        var result = new List<QuestionSetMetaDataModel>();

        // get root of question set repositories
        var root = contentQuery.ContentAtRoot()?
            .FirstOrDefault(x => x.ContentType.Alias == Questionsets.ModelTypeAlias);

        // get project record repo
        var projectRecordRuestionsetRepo = root?
            .Descendant<QuestionsetRepository>();

        // get modififcations record repo
        var modificationsRecordRuestionsetRepo = root?
            .Descendant<ModificationsQuestionsetRepository>();

        // get question sets from both repos and transform to metadata model, then add to result list
        var projectRecordQuestionSets = projectRecordRuestionsetRepo?.Children<QuestionSet>();
        var modificationsQuestionSets = modificationsRecordRuestionsetRepo?.Children<ModificationsQuestionSet>();

        if (projectRecordQuestionSets != null && projectRecordQuestionSets.Any())
        {
            result.AddRange(TransformQuestionDataModel(projectRecordQuestionSets, "ProjectRecord"));
        }

        if (modificationsQuestionSets != null && modificationsQuestionSets.Any())
        {
            result.AddRange(TransformQuestionDataModel(modificationsQuestionSets, "Modification"));
        }

        return Ok(result);
    }

    /// <summary>
    /// Transform CMS question entity into a flat metadata model
    /// that contains question and answer option data in the same list,
    /// with parent-child relationship defined by Id and ParentId properties.
    /// </summary>
    /// <param name="questionSets">Questions sets to be transformed</param>
    /// <param name="domain">Can be ProjectRecord or Modifications</param>
    /// <returns></returns>
    private IList<QuestionSetMetaDataModel> TransformQuestionDataModel(
        IEnumerable<IPublishedContent> questionSets,
        string domain)
    {
        var result = new List<QuestionSetMetaDataModel>();

        // iterate over all question sets
        foreach (var set in questionSets)
        {
            var isProjectRecordQuestionSet = set.ContentType.Alias == QuestionSet.ModelTypeAlias;
            var version = set.Value<int>("versionNumber");
            var validFrom = set.Value<DateTime>("activeFrom");
            var validTo = set.Value<DateTime>("activeTo");

            // iterate over each question in the current question set
            foreach (var question in set.Descendants<QuestionSlot>())
            {
                var section = question.Parent as Section;
                var modificationJourney = question.Parent?.Parent as ModificationJourney;
                var questionId = question.QuestionId;
                var questionContent = question.QuestionContent as Question;
                var answers = questionContent?.PossibleAnswers?.OfType<AnswerOption>(); // Ensure the collection is of type AnswerOption
                var validationRules = question.ValidationRules;
                string? parentId = null;

                // get the parent id of the question if it has one
                if (validationRules != null)
                {
                    foreach (var rule in validationRules)
                    {
                        var strongRule = rule.Content as ValidationRule;
                        var parentQuestion = strongRule?.ParentQuestion as QuestionSlot;
                        parentId = parentQuestion?.QuestionId;
                    }
                }

                // create flat metadata model for question and add to result list
                result.Add(new QuestionSetMetaDataModel
                {
                    QuestionSetVersion = version.ToString(),
                    ValidFrom = validFrom,
                    ValidTo = validTo,
                    Id = questionId,
                    Content = questionContent?.QuestionName,
                    Category = modificationJourney != null ? string.Join(" - ", modificationJourney?.Name, section?.SectionName) : section?.SectionName,
                    Domain = domain,
                    EntityType = "Question",
                    ParentId = parentId
                });

                if (answers != null)
                {
                    // iterate over answer options for the current question, create flat metadata model for each answer option and add to result list
                    foreach (var answer in answers) // Now iterating over a collection of AnswerOption
                    {
                        result.Add(new QuestionSetMetaDataModel
                        {
                            Id = answer.OptionId,
                            Content = answer.OptionName,
                            Category = modificationJourney != null ? string.Join(" - ", modificationJourney?.Name, section?.SectionName) : section?.SectionName,
                            EntityType = "AnswerOption",
                            ParentId = questionId,
                            Domain = domain,
                            QuestionSetVersion = version.ToString(),
                            ValidFrom = validFrom,
                            ValidTo = validTo
                        });
                    }
                }
            }
        }
        return result;
    }
}