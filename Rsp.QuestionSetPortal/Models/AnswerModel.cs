using Rsp.QuestionSetPortal.Models;

namespace Rsp.QuestionSetService.Models
{
    public class AnswerModel : BaseContentObject
    {
        public string? Id { get; set; }

        public string? OptionName { get; set; }
    }
}