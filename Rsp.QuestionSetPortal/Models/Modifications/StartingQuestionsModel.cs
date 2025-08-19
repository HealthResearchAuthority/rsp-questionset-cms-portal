using Rsp.QuestionSetPortal.Models.Modifications;

namespace Rsp.QuestionSetService.Models.Modifications;

public class StartingQuestionsModel
{
    public List<AreaOfChangeModel>? AreasOfChange { get; set; } = [];
}