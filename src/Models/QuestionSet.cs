namespace Rsp.QuestionSetService.Models
{
    public class QuestionSetModel
    {
        public IList<SectionModel> Sections { get; set; } = new List<SectionModel>();
    }
}