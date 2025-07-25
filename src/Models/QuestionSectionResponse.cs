namespace Rsp.QuestionSetService.Models
{
    public class QuestionSectionResponse
    {
        public string QuestionCategoryId { get; set; } = null!;
        public string? SectionId { get; set; } = null!;
        public string SectionName { get; set; } = null!;
    }
}