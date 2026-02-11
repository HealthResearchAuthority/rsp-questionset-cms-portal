namespace Rsp.QuestionSetPortal.Models.QuestionSetMetaData;

public class QuestionSetMetaDataModel
{
    public string? Id { get; set; }
    public string? Content { get; set; }
    public string? ParentId { get; set; }
    public string? QuestionSetVersion { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public string? Category { get; set; }
    public string? EntityType { get; set; }
    public string? Domain { get; set; }
}