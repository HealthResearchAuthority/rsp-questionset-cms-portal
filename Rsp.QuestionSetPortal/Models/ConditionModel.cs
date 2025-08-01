namespace Rsp.QuestionSetService.Models
{
    public class ConditionModel
    {
        public string Operator { get; set; }
        public string Mode { get; set; }
        public bool Negate { get; set; }
        public string OptionType { get; set; }
        public string Value { get; set; }
        public IList<AnswerModel> ParentOptions { get; set; }
        public string Description { get; set; }
    }
}