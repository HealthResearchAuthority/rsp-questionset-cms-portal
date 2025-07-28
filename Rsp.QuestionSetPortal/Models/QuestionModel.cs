﻿using Rsp.QuestionSetService.Models.UIContent;

namespace Rsp.QuestionSetService.Models
{
    public class QuestionModel
    {
        public string? Name { get; set; }

        /// <summary>
        /// Auto generate Id by the CMS
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Manually created Id by the user
        /// </summary>
        public string QuestionId { get; set; } = null!;
        public string Key { get; set; } = null!;
        public string? Label { get; set; }
        public string? Conformance { get; set; }
        public string? QuestionFormat { get; set; }
        public string? AnswerDataType { get; set; }
        public string? CategoryId { get; set; }
        public IList<AnswerModel> Answers { get; set; } = [];
        public IList<RuleModel> ValidationRules { get; set; } = [];
        public IList<ContentComponent> GuidanceComponents { get; set; } = [];
    }
}