using Mapster;
using Microsoft.AspNetCore.Mvc;
using Rsp.QuestionSetService.Helpers;
using Rsp.QuestionSetService.Models;
using Rsp.QuestionSetService.Models.Modifications;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetService.Controllers
{
    [ApiController]
    [Route("/umbraco/api/modificationsquestionset")]
    public class ModificationsQuestionsetController : ControllerBase
    {
        private readonly IPublishedContentQuery _contentQuery;

        public ModificationsQuestionsetController(IPublishedContentQuery contentQuery)
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

        [HttpGet("GetStartingQuestions")]
        public StartingQuestionsModel GetStartingQuestions()
        {
            var result = new StartingQuestionsModel();
            var activeQuestionSet = GetActiveModificationsQuestionset();

            if (activeQuestionSet != null)
            {
                var questionSetNode = _contentQuery.Content(activeQuestionSet.Key);

                if (questionSetNode != null)
                {
                    var areaOfChange = questionSetNode.FirstChild<AreaOfChangeQuestion>();
                    var specificChange = questionSetNode.FirstChild<SpecificChangeQuestion>();

                    if (areaOfChange != null)
                    {
                        var answerOptions = areaOfChange.Children<AnswerOption>();
                        result.AreaOfChange = new QuestionModel
                        {
                            Id = areaOfChange.Key.ToString(),
                            Label = areaOfChange.Text,
                            Answers = answerOptions != null ? answerOptions.Select(x => x.Adapt<AnswerModel>()).ToList() : []
                        };
                    }

                    if (specificChange != null)
                    {
                        var answerOptions = specificChange.Children<AnswerOption>();
                        result.SpecificChange = new QuestionModel
                        {
                            Id = specificChange.Key.ToString(),
                            Label = specificChange.Text,
                            Answers = answerOptions != null ? answerOptions.Select(x => x.Adapt<AnswerModel>()).ToList() : []
                        };
                    }
                }
            }

            return result;
        }

        [HttpGet("GetModificationsJourney")]
        public QuestionSetModel GetModificationsJourney(string specificChangeId)
        {
            var result = new QuestionSetModel();

            var activeQuestionSet = GetActiveModificationsQuestionset();
            if (activeQuestionSet != null)
            {
                var journeys = activeQuestionSet.Children<ModificationJourney>();
                var activeJourney = journeys?.FirstOrDefault(x => x.Condition != null && x.Condition.Any(x => x.Key.ToString() == specificChangeId));

                if (activeJourney != null)
                {
                    foreach (var section in activeJourney.Children<Section>())
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

        private IPublishedContent? GetActiveModificationsQuestionset()
        {
            var questionsets = _contentQuery.ContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == Questionsets.ModelTypeAlias);

            if (questionsets != null)
            {
                var modRepo = questionsets.FirstChild<ModificationsQuestionsetRepository>();

                if (modRepo != null)
                {
                    var activeQuestionSet = modRepo.Value<IPublishedContent>("activeQuestionset");

                    return activeQuestionSet;
                }
            }
            return null;
        }
    }
}