using Microsoft.AspNetCore.Mvc;
using Rsp.QuestionSetPortal.Helpers;
using Rsp.QuestionSetPortal.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Controllers;

[ApiController]
[Route("/umbraco/api/projectRecordQuestionset")]
public class ProjectRecordQuestionsetController : ControllerBase
{
    private readonly IPublishedContentQuery _contentQuery;

    public ProjectRecordQuestionsetController(IPublishedContentQuery contentQuery)
    {
        _contentQuery = contentQuery;
    }

    [HttpGet("getQuestionSet")]
    public QuestionSetModel GetQuestionSet(string? sectionId = null, string? questionSetId = null, bool preview = false)
    {
        var result = new QuestionSetModel();

        if (!string.IsNullOrEmpty(sectionId))
        {
            var activeQuestionSet = GetQuestionsetByVersion(preview);

            var section = activeQuestionSet?.Descendants<Section>().FirstOrDefault(x => x.SectionId != null &&
                x.SectionId.Equals(sectionId, StringComparison.InvariantCultureIgnoreCase));

            if (activeQuestionSet == null || section == null)
            {
                return result;
            }

            ContentHelpers.PopulateGeneralQuestionSetMetadata(result, activeQuestionSet);

            var sectionModel = ContentHelpers.PopulateSectionModel(section);

            sectionModel.Questions = ContentHelpers.TransformQuestions(section, result.Version);

            result.Sections.Add(sectionModel);
        }
        else
        {
            if (string.IsNullOrEmpty(questionSetId))
            {
                // no id passed so go ahead and get the active questionset
                var activeQuestionSet = GetQuestionsetByVersion(preview);

                if (activeQuestionSet != null)
                {
                    questionSetId = activeQuestionSet.Key.ToString();

                    ContentHelpers.PopulateGeneralQuestionSetMetadata(result, activeQuestionSet);
                }
            }

            if (!string.IsNullOrEmpty(questionSetId))
            {
                var questionSet = _contentQuery.Content(questionSetId) as QuestionSet;

                ContentHelpers.PopulateGeneralQuestionSetMetadata(result, questionSet);

                var sections = questionSet?.Descendants<Section>();

                if (sections != null)
                {
                    foreach (var section in sections)
                    {
                        var sectionModel = ContentHelpers.PopulateSectionModel(section);

                        sectionModel.Questions = ContentHelpers.TransformQuestions(section, result.Version);

                        result.Sections.Add(sectionModel);
                    }
                }
            }
        }

        return result;
    }

    [HttpGet("getQuestionSections")]
    public IEnumerable<QuestionSectionResponse> GetQuestionSections(bool preview)
    {
        var result = new List<QuestionSectionResponse>();

        var questionSet = GetQuestionsetByVersion(preview);

        if (questionSet != null)
        {
            foreach (var section in questionSet.Descendants<Section>())
            {
                var category = section.Category as Category;
                result.Add(new QuestionSectionResponse
                {
                    SectionId = section.SectionId,
                    SectionName = section?.SectionName?.ToString(),
                    QuestionCategoryId = category?.CategoryId,
                });
            }
        }

        return result;
    }

    [HttpGet("getNextQuestionSection")]
    public QuestionSectionResponse? GetNextQuestionSection(string currentSectionId, bool preview)
    {
        if (string.IsNullOrEmpty(currentSectionId))
        {
            return null;
        }

        var questionset = GetQuestionsetByVersion(preview);
        var currentSection = questionset?
            .Descendants<Section>()
            .FirstOrDefault(x =>
                !string.IsNullOrEmpty(x.SectionId) &&
                 x.SectionId
                .Equals(currentSectionId, StringComparison.InvariantCultureIgnoreCase)
            );

        if (currentSection != null)
        {
            var allSections = currentSection.Parent<QuestionsetContentContainer>()?.Children<Section>();

            if (allSections != null)
            {
                var currentSectionIndex = allSections.FindIndex(x => x.Key == currentSection.Key);
                if (allSections.ElementAtOrDefault(currentSectionIndex + 1) != null)
                {
                    var nextSection = allSections.ElementAtOrDefault(currentSectionIndex + 1);
                    var category = nextSection?.Category as Category;

                    var sectionModel = new QuestionSectionResponse
                    {
                        SectionId = nextSection?.SectionId,
                        SectionName = nextSection?.SectionName?.ToString(),
                        QuestionCategoryId = category?.CategoryId,
                    };

                    return sectionModel;
                }
            }
        }

        return null;
    }

    [HttpGet("getPreviousQuestionSection")]
    public QuestionSectionResponse? GetPreviousQuestionSection(string currentSectionId, bool preview)
    {
        if (string.IsNullOrEmpty(currentSectionId))
        {
            return null;
        }

        var questionset = GetQuestionsetByVersion(preview);
        var currentSection = questionset?
            .Descendants<Section>()
            .FirstOrDefault(x =>
                !string.IsNullOrEmpty(x.SectionId) &&
                 x.SectionId
                .Equals(currentSectionId, StringComparison.InvariantCultureIgnoreCase)
            );

        if (currentSection != null)
        {
            var allSections = currentSection.Parent<QuestionsetContentContainer>()?.Children<Section>();

            if (allSections != null)
            {
                var currentSectionIndex = allSections.FindIndex(x => x.Key == currentSection.Key);
                if (allSections.ElementAtOrDefault(currentSectionIndex - 1) != null)
                {
                    var prevSection = allSections.ElementAtOrDefault(currentSectionIndex - 1);
                    var category = prevSection?.Category as Category;

                    var sectionModel = new QuestionSectionResponse
                    {
                        SectionId = prevSection?.SectionId,
                        SectionName = prevSection?.SectionName?.ToString(),
                        QuestionCategoryId = category?.CategoryId,
                    };

                    return sectionModel;
                }
            }
        }

        return null;
    }

    [HttpGet("getQuestionCategories")]
    public IEnumerable<QuestionCategoryResponse> GetQuestionCategories(bool preview)
    {
        var result = new List<QuestionCategoryResponse>();
        var questionSet = GetQuestionsetByVersion(preview: preview);

        if (questionSet != null)
        {
            foreach (var category in questionSet.Descendants<Category>())
            {
                result.Add(new QuestionCategoryResponse
                {
                    CategoryId = category.CategoryId!,
                    CategoryName = category.Name,
                    VersionId = questionSet.VersionNumber.ToString()
                });
            }
        }

        return result;
    }

    private QuestionSet? GetQuestionsetByVersion(bool preview = false)
    {
        var questionsetRepo = _contentQuery.ContentAtRoot()?
            .FirstOrDefault(x => x.ContentType.Alias == Questionsets.ModelTypeAlias)?
            .Descendant<QuestionsetRepository>();

        if (questionsetRepo != null)
        {
            if (preview)
            {
                // get preview questionset
                var previewQuestionSet = questionsetRepo.Value<IPublishedContent>("previewQuestionset") as QuestionSet;
                return previewQuestionSet;
            }

            // get active questionset because version is not specified
            var activeQuestionSet = questionsetRepo.Value<IPublishedContent>("activeQuestionset") as QuestionSet;
            return activeQuestionSet;
        }

        // questionset repository does not exist
        return null;
    }
}