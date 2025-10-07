using Microsoft.AspNetCore.Mvc;
using Rsp.QuestionSetPortal.DTOs.Requests;
using Rsp.QuestionSetPortal.Services;

namespace Rsp.QuestionSetPortal.Controllers;

[ApiController]
[Route("/umbraco/api/[controller]/[action]")]
public class ModificationsRankingController
(
    IRankingCalculationService rankingCalculationService
) : ControllerBase
{
    [HttpGet]
    public async Task<RankingOfChangeResponse> GetModificationRanking([FromQuery] RankingOfChangeRequest request)
    {
        // calculate modification ranking
        var modificationRanking =
            rankingCalculationService
                .GetModificationRanking
                (
                    request.SpecificAreaOfChangeId,
                    request.Applicability,
                    request.ProjectType,
                    request.Version
                );

        // calculate category ranking
        var categoryRanking =
            rankingCalculationService
                .GetCategoryRanking
                (
                    request.SpecificAreaOfChangeId,
                    request.IsNHSInvolved,
                    request.IsNonNHSInvolved,
                    request.NhsOrganisationsAffected,
                    request.NhsResourceImplicaitons,
                    request.Version
                );

        // calculate review type
        var reviewType = rankingCalculationService.GetReviewType(request.SpecificAreaOfChangeId, request.Version);

        await Task.WhenAll(modificationRanking, categoryRanking, reviewType);

        return new RankingOfChangeResponse
        {
            ModificationType = modificationRanking.Result,
            Categorisation = categoryRanking.Result,
            ReviewType = reviewType.Result,
        };
    }
}