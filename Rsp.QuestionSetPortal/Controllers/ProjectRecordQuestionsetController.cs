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

        [HttpGet("GetQuestionSet")]
        public QuestionSetModel GetQuestionSet(string? sectionId = null, string? questionSetId = null)
        {
            var result = new QuestionSetModel();

            if (!string.IsNullOrEmpty(sectionId))
            {
                var section = _contentQuery.Content(sectionId) as Section;
                var questionSet = section?.AncestorOrSelf<QuestionSet>();

                if (questionSet == null)
                {
                    return result;
                }

                PopulateGeneralQuestionSetMetadata(result, questionSet);

                if (section != null)
                {
                    var sectionModel = new SectionModel
                    {
                        SectionName = section.SectionName,
                        Id = section.Key.ToString(),
                        SectionId = !string.IsNullOrEmpty(section.SectionId) ? section.SectionId : null,
                        GuidanceComponents = section.GuidanceContent != null ? ContentHelpers.TransformUiComponent(section.GuidanceContent) : []
                    };

                    sectionModel.Questions = ContentHelpers.TransformQuestions(section);

                    result.Sections.Add(sectionModel);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(questionSetId))
                {
                    // no id passed so go ahead and get the active questionset
                    var activeQuestionSet = GetActiveQuestionset();

                    if (activeQuestionSet != null)
                    {
                        questionSetId = activeQuestionSet.Key.ToString();

                        PopulateGeneralQuestionSetMetadata(result, activeQuestionSet);
                    }
                }

                if (!string.IsNullOrEmpty(questionSetId))
                {
                    var questionSet = _contentQuery.Content(questionSetId) as QuestionSet;

                    PopulateGeneralQuestionSetMetadata(result, questionSet);

                    var sections = questionSet?.Descendants<Section>();

                    if (sections != null)
                    {
                        foreach (var section in sections)
                        {
                            var sectionModel = new SectionModel
                            {
                                SectionName = section.SectionName,
                                Id = section.Key.ToString(),
                                SectionId = !string.IsNullOrEmpty(section.SectionId) ? section.SectionId : null,
                                GuidanceComponents = section.GuidanceContent != null ? ContentHelpers.TransformUiComponent(section.GuidanceContent) : []
                            };

                            sectionModel.Questions = ContentHelpers.TransformQuestions(section);

                            result.Sections.Add(sectionModel);
                        }
                    }
                }
            }

            return result;
        }

        [HttpGet("GetQuestionSections")]
        public IEnumerable<QuestionSectionResponse> GetQuestionSections()
        {
            var result = new List<QuestionSectionResponse>();

            var questionSet = GetActiveQuestionset();

            if (questionSet != null)
            {
                foreach (var section in questionSet.Children<Section>())
                {
                    result.Add(new QuestionSectionResponse
                    {
                        SectionId = section.Key.ToString(),
                        SectionName = section?.SectionName?.ToString(),
                        QuestionCategoryId = section?.Key.ToString(),
                    });
                }
            }

            return result;
        }

        [HttpGet("GetNextQuestionSection")]
        public QuestionSectionResponse? GetNextQuestionSection(string currentSectionId)
        {
            if (string.IsNullOrEmpty(currentSectionId))
            {
                return null;
            }

            var currentSection = _contentQuery.Content(currentSectionId);

            if (currentSection != null)
            {
                var allSections = currentSection.Parent<QuestionsetContentContainer>()?.Children<Section>();

                if (allSections != null)
                {
                    var currentSectionIndex = allSections.FindIndex(x => x.Key == currentSection.Key);
                    if (allSections.ElementAtOrDefault(currentSectionIndex + 1) != null)
                    {
                        var nextSection = allSections.ElementAtOrDefault(currentSectionIndex + 1);
                        var sectionModel = new QuestionSectionResponse
                        {
                            SectionId = nextSection?.Key.ToString(),
                            SectionName = nextSection?.SectionName?.ToString(),
                            QuestionCategoryId = nextSection?.Key.ToString(),
                        };

                        return sectionModel;
                    }
                }
            }

            return null;
        }

        [HttpGet("GetPreviousQuestionSection")]
        public QuestionSectionResponse? GetPreviousQuestionSection(string currentSectionId)
        {
            if (string.IsNullOrEmpty(currentSectionId))
            {
                return null;
            }

            var currentSection = _contentQuery.Content(currentSectionId);

            if (currentSection != null)
            {
                var allSections = currentSection.Parent<QuestionsetContentContainer>()?.Children<Section>();

                if (allSections != null)
                {
                    var currentSectionIndex = allSections.FindIndex(x => x.Key == currentSection.Key);
                    if (allSections.ElementAtOrDefault(currentSectionIndex - 1) != null)
                    {
                        var nextSection = allSections.ElementAtOrDefault(currentSectionIndex - 1);
                        var sectionModel = new QuestionSectionResponse
                        {
                            SectionId = nextSection?.Key.ToString(),
                            SectionName = nextSection?.SectionName?.ToString(),
                            QuestionCategoryId = nextSection?.Key.ToString(),
                        };

                        return sectionModel;
                    }
                }
            }

            return null;
        }

        [HttpGet("GetQuestionCategories")]
        public IEnumerable<QuestionCategoryResponse> GetQuestionCategories()
        {
            var result = new List<QuestionCategoryResponse>();
            var questionSet = GetActiveQuestionset();

            if (questionSet != null)
            {
                foreach (var category in questionSet.Descendants<Category>())
                {
                    result.Add(new QuestionCategoryResponse
                    {
                        CategoryId = category.Key.ToString(),
                        CategoryName = category.Name
                    });
                }
            }

            return result;
        }

        private static void PopulateGeneralQuestionSetMetadata(QuestionSetModel model, QuestionSet questionset)
        {
            model.Id = questionset.Key.ToString();
            model.Version = questionset.VersionNumber.ToString();
            model.Status = questionset.Status;
            model.ActiveFrom = questionset.ActiveFrom != DateTime.MinValue ? questionset?.ActiveFrom : null;
            model.ActiveTo = questionset?.ActiveTo != DateTime.MinValue ? questionset?.ActiveTo : null;
        }

        private QuestionSet? GetActiveQuestionset()
        {
            var questionsetRepo = _contentQuery.ContentAtRoot()?.FirstOrDefault()?.Descendant<QuestionsetRepository>();

            if (questionsetRepo != null)
            {
                var activeQuestionSet = questionsetRepo.Value<IPublishedContent>("activeQuestionset") as QuestionSet;
                return activeQuestionSet;
            }
            return null;
        }
    }
}