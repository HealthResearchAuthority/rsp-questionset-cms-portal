using Rsp.QuestionSetPortal.Models.Modifications;
using CategoryRank = Rsp.QuestionSetPortal.Models.Modifications.CategoryRank;

namespace Rsp.QuestionSetPortal.Services;

public interface IRankingCalculationService
{
    Task<CategoryRank> GetCategoryRanking(Guid specificAreaOfChangeId, bool isNhsInvolved, bool isNonNhsInvolved, string? nhsOrgsAffected, bool resourceImplications, string? version = null);

    Task<ModificationRank> GetModificationRanking(Guid specificAreaOfChangeId, string applicability, string projectType, string? version = null);

    Task<string> GetReviewType(Guid specificAreaOfChangeId, string? version = null);
}