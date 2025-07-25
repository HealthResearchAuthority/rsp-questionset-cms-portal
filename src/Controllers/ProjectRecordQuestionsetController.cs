using Microsoft.AspNetCore.Mvc;
using Rsp.QuestionSetService.Helpers;
using Rsp.QuestionSetService.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetService.Controllers
{
    [ApiController]
    [Route("/umbraco/api/projectrecordquestionset")]
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
                if (section != null)
                {
                    var sectionModel = new SectionModel
                    {
                        SectionName = section.SectionName,
                        Id = section.Key.ToString(),
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
                    var questionsetRepo = _contentQuery.ContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == QuestionsetRepository.ModelTypeAlias);

                    if (questionsetRepo != null)
                    {
                        var activeQuestionSet = questionsetRepo.Value<IPublishedContent>("activeQuestionset");

                        if (activeQuestionSet != null)
                        {
                            questionSetId = activeQuestionSet.Key.ToString();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(questionSetId))
                {
                    var questionSet = _contentQuery.Content(questionSetId);

                    if (questionSet != null)
                    {
                        foreach (var section in questionSet.Children<Section>())
                        {
                            var sectionModel = new SectionModel
                            {
                                SectionName = section.SectionName,
                                Id = section.Key.ToString(),
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

            var questionsetRepo = _contentQuery.ContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == QuestionsetRepository.ModelTypeAlias);

            if (questionsetRepo != null)
            {
                var activeQuestionSet = questionsetRepo.Value<IPublishedContent>("activeQuestionset");

                if (activeQuestionSet != null)
                {
                    var questionSet = _contentQuery.Content(activeQuestionSet.Key);

                    foreach (var section in questionSet.Children<Section>())
                    {
                        result.Add(new QuestionSectionResponse
                        {
                            SectionId = section.Key.ToString(),
                            SectionName = section.SectionName.ToString(),
                            QuestionCategoryId = section.Key.ToString(),
                        });
                    }
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
                var allSections = currentSection.Parent<QuestionSet>()?.Children<Section>();

                if (allSections != null)
                {
                    var currentSectionIndex = allSections.FindIndex(x => x.Key == currentSection.Key);
                    if (allSections.ElementAtOrDefault(currentSectionIndex + 1) != null)
                    {
                        var nextSection = allSections.ElementAtOrDefault(currentSectionIndex + 1);
                        var sectionModel = new QuestionSectionResponse
                        {
                            SectionId = nextSection.Key.ToString(),
                            SectionName = nextSection.SectionName.ToString(),
                            QuestionCategoryId = nextSection.Key.ToString(),
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
                var allSections = currentSection.Parent<QuestionSet>()?.Children<Section>();

                if (allSections != null)
                {
                    var currentSectionIndex = allSections.FindIndex(x => x.Key == currentSection.Key);
                    if (allSections.ElementAtOrDefault(currentSectionIndex - 1) != null)
                    {
                        var nextSection = allSections.ElementAtOrDefault(currentSectionIndex - 1);
                        var sectionModel = new QuestionSectionResponse
                        {
                            SectionId = nextSection.Key.ToString(),
                            SectionName = nextSection.SectionName.ToString(),
                            QuestionCategoryId = nextSection.Key.ToString(),
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

            var questionsetRepo = _contentQuery.ContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == QuestionsetRepository.ModelTypeAlias);

            if (questionsetRepo != null)
            {
                var activeQuestionSet = questionsetRepo.Value<IPublishedContent>("activeQuestionset");

                if (activeQuestionSet != null)
                {
                    var questionSet = _contentQuery.Content(activeQuestionSet.Key);

                    foreach (var section in questionSet.Children<Section>())
                    {
                        result.Add(new QuestionCategoryResponse
                        {
                            CategoryId = section.Key.ToString(),
                            CategoryName = section.SectionName.ToString()
                        });
                    }
                }
            }

            return result;
        }

        private object MapContent(IPublishedContent content)
        {
            var customPropertyAliases = content.ContentType
                .PropertyTypes
                .Select(p => p.Alias)
                .ToHashSet();

            return new
            {
                id = content.Id,
                name = content.Name,
                contentType = content.ContentType.Alias,
                properties = content.Properties.Where(x => customPropertyAliases.Contains(x.Alias)).ToDictionary(
                    p => p.Alias,
                    p => p.GetValue()
                ),
                children = content.Children.Select(MapContent)
            };
        }
    }
}