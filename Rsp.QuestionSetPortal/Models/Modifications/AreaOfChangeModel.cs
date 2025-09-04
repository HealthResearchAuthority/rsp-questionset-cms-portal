using System.Text.Json.Serialization;

namespace Rsp.QuestionSetPortal.Models.Modifications;

public class AreaOfChangeModel : AnswerModel
{
    [JsonPropertyOrder(int.MaxValue)]
    public List<AnswerModel> SpecificAreasOfChange { get; set; } = [];
}