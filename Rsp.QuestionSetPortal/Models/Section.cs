using Rsp.QuestionSetService.Models.UIContent;

namespace Rsp.QuestionSetService.Models
{
    public class SectionModel
    {
        public string SectionName { get; set; }
        public string Id { get; set; }
        public IList<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
        public IList<ContentComponent> GuidanceComponents { get; set; } = [];
    }
}