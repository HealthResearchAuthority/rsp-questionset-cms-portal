using Mapster;
using Rsp.QuestionSetService.Models;
using Rsp.QuestionSetService.Models.UIContent;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetService.Helpers;

public static class ContentHelpers
{
    public static IList<QuestionModel> TransformQuestions(Section section)
    {
        var result = new List<QuestionModel>();

        foreach (var questionSlot in section.Children<QuestionSlot>())
        {
            var associatedQuestion = questionSlot.QuestionContent as Question;
            var questionCategory = questionSlot.Category?.FirstOrDefault() as Category;

            var questionModel = associatedQuestion.Adapt<QuestionModel>();

            questionModel.CategoryId = questionCategory?.CategoryId;
            questionModel.Answers = TransformAnswers(associatedQuestion);
            questionModel.ValidationRules = TransformValidationRules(questionSlot);
            questionModel.Name = associatedQuestion.QuestionName;
            questionModel.GuidanceComponents = associatedQuestion.GuidanceContent != null ? TransformUiComponent(associatedQuestion.GuidanceContent) : [];

            result.Add(questionModel);
        }

        return result;
    }

    public static IList<ContentComponent> TransformUiComponent(BlockListModel content)
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

    public static IList<AnswerModel> TransformAnswers(Question question)
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

    public static IList<RuleModel> TransformValidationRules(QuestionSlot question)
    {
        var result = new List<RuleModel>();

        var rules = question.ValidationRules;
        if (rules != null)
        {
            foreach (var rule in rules)
            {
                var strongRule = rule.Content as ValidationRule;

                if (strongRule != null)
                {
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
        }

        return result;
    }
}