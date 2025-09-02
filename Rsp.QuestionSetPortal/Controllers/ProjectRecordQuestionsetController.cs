using Microsoft.AspNetCore.Mvc;
using Rsp.QuestionSetService.Helpers;
using Rsp.QuestionSetService.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetService.Controllers
{
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
        public QuestionSetModel GetQuestionSet(string? sectionId = null, string? questionSetId = null, string? version = null)
        {
            var result = new QuestionSetModel();

            if (!string.IsNullOrEmpty(sectionId))
            {
                var activeQuestionSet = GetQuestionsetByVersion(version);

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
                    var activeQuestionSet = GetQuestionsetByVersion(version);

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
        public IEnumerable<QuestionSectionResponse> GetQuestionSections(string? version = null)
        {
            var result = new List<QuestionSectionResponse>();

            var questionSet = GetQuestionsetByVersion();

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
        public QuestionSectionResponse? GetNextQuestionSection(string currentSectionId, string? version = null)
        {
            if (string.IsNullOrEmpty(currentSectionId))
            {
                return null;
            }

            var questionset = GetQuestionsetByVersion(version);
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
        public QuestionSectionResponse? GetPreviousQuestionSection(string currentSectionId, string? version = null)
        {
            if (string.IsNullOrEmpty(currentSectionId))
            {
                return null;
            }

            var questionset = GetQuestionsetByVersion(version);
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
        public IEnumerable<QuestionCategoryResponse> GetQuestionCategories(string? version = null)
        {
            var result = new List<QuestionCategoryResponse>();
            var questionSet = GetQuestionsetByVersion(version);

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

        private QuestionSet? GetQuestionsetByVersion(string? version = null)
        {
            var questionsetRepo = _contentQuery.ContentAtRoot()?.FirstOrDefault()?.Descendant<QuestionsetRepository>();
            if (questionsetRepo != null)
            {
                if (string.IsNullOrEmpty(version))
                {
                    // get active questionset because version is not specified
                    var activeQuestionSet = questionsetRepo.Value<IPublishedContent>("activeQuestionset") as QuestionSet;
                    return activeQuestionSet;
                }
                else
                {
                    // version is specified so get questionset by version
                    var questionset = questionsetRepo
                        .Children<QuestionSet>()?
                        .FirstOrDefault(x =>
                            x.VersionNumber
                            .ToString()
                            .Equals(version, StringComparison.InvariantCultureIgnoreCase)
                        );

                    return questionset;
                }
            }

            // questionset repository does not exist
            return null;
        }
    }
}