using Rsp.QuestionSetPortal.Models;

namespace Rsp.QuestionSetService.Models
{
    public class AnswerModel : BaseContentObject
    {
        public string? OptionName { get; set; }
        public string? Id { get; set; }
    }
}