using Rsp.QuestionSetPortal.Models;
using Rsp.QuestionSetService.Models.UIContent;

namespace Rsp.QuestionSetService.Models
{
    public class QuestionModel : BaseContentObject
    {
        public string? Name { get; set; }
        public string? ShortName { get; set; }

        /// <summary>
        /// Manually created Id by the user
        /// </summary>
        public string? Id { get; set; } = null!;

        public string? Label { get; set; }
        public string? Conformance { get; set; }
        public string? QuestionFormat { get; set; }
        public string? AnswerDataType { get; set; }
        public string? CategoryId { get; set; }

        /// <summary>
        /// Represents the version of the questionset
        /// </summary>
        public string? Version { get; set; }

        public IList<AnswerModel> Answers { get; set; } = [];
        public IList<RuleModel> ValidationRules { get; set; } = [];
        public IList<ContentComponent> GuidanceComponents { get; set; } = [];
    }
}