namespace Rsp.QuestionSetPortal.Models;

public class QuestionSectionResponse
{
    public string? QuestionCategoryId { get; set; }
    public string? SectionId { get; set; }
    public string? SectionName { get; set; }
    public string? StaticViewName { get; set; }
    public bool IsMandatory { get; set; }
}