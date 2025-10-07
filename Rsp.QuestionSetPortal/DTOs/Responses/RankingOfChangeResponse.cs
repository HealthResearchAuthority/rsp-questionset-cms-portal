using Rsp.QuestionSetPortal.Models.Modifications;

namespace Rsp.QuestionSetPortal.DTOs.Requests;

public class RankingOfChangeResponse
{
    public CategoryRank Categorisation { get; set; } = new();

    public ModificationRank ModificationType { get; set; } = new();

    public string ReviewType { get; set; } = null!;
}