using Mapster;
using Microsoft.AspNetCore.Mvc;
using Rsp.QuestionSetService.Models;
using Rsp.QuestionSetService.Models.UIContent;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetService.Controllers
{
    [ApiController]
    [Route("/umbraco/api/nestedcontent")]
    public class NestedContentController : ControllerBase
    {
        private readonly IPublishedContentQuery _contentQuery;

        public NestedContentController(IPublishedContentQuery contentQuery)
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
                        GuidanceComponents = section.GuidanceContent != null ? TransformUiComponent(section.GuidanceContent) : []
                    };

                    sectionModel.Questions = TransformQuestions(section);

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
                                GuidanceComponents = section.GuidanceContent != null ? TransformUiComponent(section.GuidanceContent) : []
                            };

                            sectionModel.Questions = TransformQuestions(section);

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

        private IList<QuestionModel> TransformQuestions(Section section)
        {
            var result = new List<QuestionModel>();

            foreach (var question in section.Children<Question>())
            {
                var questionModel = question.Adapt<QuestionModel>();

                questionModel.Answers = TransformAnswers(question);
                questionModel.ValidationRules = TransformValidationRules(question);
                questionModel.Name = question.QuestionName;
                questionModel.GuidanceComponents = question.GuidanceContent != null ? TransformUiComponent(question.GuidanceContent) : [];

                result.Add(questionModel);
            }
            return result;
        }

        private IList<ContentComponent> TransformUiComponent(BlockListModel content)
        {
            var result = new List<ContentComponent>();
            foreach (var block in content)
            {
                switch (block.Content)
                {
                    case AccordionComponent component:

                        if (component.Items != null)
                        {
                            var items = component.Items.Select(x => new AccordionComponentItemModel
                            {
                                Title = x.Content.Value<string>("title"),
                                Value = x.Content.Value<string>("value")
                            })?.ToList();
                            result.Add(new AccordionComponentModel { ContentType = component.ContentType.Alias, Items = items });
                        }

                        break;

                    case DetailsComponent component:

                        result.Add(new DetailsComponentModel
                        {
                            ContentType = component.ContentType.Alias,
                            Title = component.Title,
                            Value = component.Value,
                        });

                        break;

                    case BodyTextComponent component:

                        result.Add(new BodyTextComponentModel
                        {
                            ContentType = component.ContentType.Alias,
                            Value = component.Value,
                        });

                        break;

                    case TabsComponent component:

                        if (component.Items != null)
                        {
                            var items = component.Items.Select(x => new TabComponentItemModel
                            {
                                Title = x.Content.Value<string>("title"),
                                Value = x.Content.Value<string>("value")
                            })?.ToList();
                            result.Add(new TabsComponentModel { ContentType = component.ContentType.Alias, Items = items });
                        }

                        break;
                }
            }
            return result;
        }

        private IList<AnswerModel> TransformAnswers(Question question)
        {
            var result = new List<AnswerModel>();

            var answers = question.PossibleAnswers;
            if (answers != null)
            {
                var questionModel = answers.Select(x => x.Adapt<AnswerModel>());

                result.AddRange(questionModel);
            }

            return result;
        }

        private IList<RuleModel> TransformValidationRules(Question question)
        {
            var result = new List<RuleModel>();

            var rules = question.ValidationRules;
            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    var strongRule = rule.Content as ValidationRule;
                    var ruleModel = strongRule.Adapt<RuleModel>();

                    ruleModel.QuestionId = question.Id.ToString();
                    ruleModel.ParentQuestion = strongRule.ParentQuestion.Adapt<QuestionModel>();
                    if (ruleModel.ParentQuestion != null)
                    {
                        ruleModel.ParentQuestion.Name = strongRule.ParentQuestion.Value<string>("questionName");
                    }

                    ruleModel.Conditions = new List<ConditionModel>();

                    foreach (var condition in strongRule.Conditions)
                    {
                        var strongCondition = condition.Content as ValidationCondition;

                        var conditionModel = strongCondition.Adapt<ConditionModel>();

                        if (strongCondition?.ParentOptions != null)
                        {
                            conditionModel.ParentOptions = strongCondition.ParentOptions.Select(x => x.Adapt<AnswerModel>()).ToList();
                        }

                        ruleModel.Conditions.Add(conditionModel);
                    }

                    result.Add(ruleModel);
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