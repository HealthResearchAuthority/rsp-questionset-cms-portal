using Rsp.QuestionSetPortal.Models.Modifications;
using Umbraco.Cms.Web.Common.PublishedModels;
using CategoryRank = Rsp.QuestionSetPortal.Models.Modifications.CategoryRank;

namespace Rsp.QuestionSetPortal.Services;

// Service responsible for calculating rankings and review types for modifications and categories
public class RankingCalculationService
(
    IModificationQuestionSetService questionSetService
) : IRankingCalculationService
{
    /// <summary>
    /// Gets the modification ranking for a specific area of change, applicability, and project type.
    /// </summary>
    /// <param name="specificAreaOfChangeId">The unique identifier for the specific area of change.</param>
    /// <param name="applicability">The applicability value to match.</param>
    /// <param name="projectType">The project type value to match.</param>
    /// <param name="version">Optional version of the question set.</param>
    /// <returns>A <see cref="ModificationRank"/> object with the calculated ranking.</returns>
    public Task<ModificationRank> GetModificationRanking(Guid specificAreaOfChangeId, string applicability, string projectType, string? version = null)
    {
        // Retrieve the active question set for the given version
        var activeQuestionSet = questionSetService.GetQuestionsetByVersion(version);

        // Get the ModificationRanking childs from the question set
        var rankings = activeQuestionSet?.Children<ModificationRanking>();

        // get the modification ranking of type ModificationType from the rankings
        var modificationTypes =
            from ranking in rankings
            let children = ranking.Children<ModificationType>()
            where children?.Any() is true
            from child in children
            select child;

        var modificationRank = new ModificationRank();

        // If no ranking or no ModificationType children, return default ModificationRank
        if (modificationTypes?.Any() is false)
        {
            return Task.FromResult(modificationRank);
        }

        // Find the ModificationType that matches the criteria
        var modificationType =
        (
            from mt in modificationTypes
            from c in mt.Criteria!
            let criteria = c.Content as ModificationTypeCriteria
            where criteria.SpecificAreaOfChange?.Any(x => x.Key.ToString() == specificAreaOfChangeId.ToString()) is true &&
                  criteria.Applicability == applicability &&
                  criteria.ProjectType == projectType
            select mt
        ).SingleOrDefault();

        // If found, set the Substantiality property on the result
        if (modificationType is not null)
        {
            modificationRank.Substantiality = modificationType.Substantiality!;
            modificationRank.Order = modificationType.Order;
        }

        return Task.FromResult(modificationRank);
    }

    /// <summary>
    /// Gets the category ranking for a specific area of change and related parameters.
    /// </summary>
    /// <param name="specificAreaOfChangeId">The unique identifier for the specific area of change.</param>
    /// <param name="isNhsInvolved">Indicates if NHS is involved.</param>
    /// <param name="isNonNhsInvolved">Indicates if non-NHS is involved.</param>
    /// <param name="nhsOrgsAffected">The affected NHS organizations.</param>
    /// <param name="resourceImplications">Indicates if there are resource implications.</param>
    /// <param name="version">Optional version of the question set.</param>
    /// <returns>A <see cref="CategoryRank"/> object with the calculated category ranking.</returns>
    public Task<CategoryRank> GetCategoryRanking(Guid specificAreaOfChangeId, bool isNhsInvolved, bool isNonNhsInvolved, string? nhsOrgsAffected, bool resourceImplications, string? version = null)
    {
        // Retrieve the active question set for the given version
        var activeQuestionSet = questionSetService.GetQuestionsetByVersion(version);

        // Get the ModificationRanking childs from the question set
        var rankings = activeQuestionSet?.Children<ModificationRanking>();

        // get the modification ranking of type Categorisation from the rankings
        var categorisations =
            from ranking in rankings
            let children = ranking.Children<Categorisation>()
            where children?.Any() is true
            from child in children
            select child;

        var categoryRank = new CategoryRank();

        // If no ranking or no Categorisation children, return default CategoryRank
        if (categorisations?.Any() is false)
        {
            return Task.FromResult(categoryRank);
        }

        // Find the Categorisation that matches the criteria
        var categorisation =
        (
            from cat in categorisations
            from c in cat.Criteria!
            let criteria = c.Content as CategorisationCriteria
            where criteria.SpecificAreaOfChange?.Any(x => x.Key.ToString() == specificAreaOfChangeId.ToString()) is true &&
                  criteria.IsNhsinvolved == isNhsInvolved &&
                  criteria.IsNonNhsinvolved == isNonNhsInvolved &&
                  criteria.NhsOrganisationsAffected == nhsOrgsAffected &&
                  criteria.NhsResourceImplications == resourceImplications
            select cat
        ).SingleOrDefault();

        // If found, set the Category property on the result
        if (categorisation is not null)
        {
            categoryRank.Category = categorisation.Category!;
            categoryRank.Order = categorisation.Order;
        }

        return Task.FromResult(categoryRank);
    }

    /// <summary>
    /// Gets the review type for a specific area of change.
    /// </summary>
    /// <param name="specificAreaOfChangeId">The unique identifier for the specific area of change.</param>
    /// <param name="version">Optional version of the question set.</param>
    /// <returns>The review type value as a string, or empty if not found.</returns>
    public Task<string> GetReviewType(Guid specificAreaOfChangeId, string? version = null)
    {
        // Retrieve the active question set for the given version
        var activeQuestionSet = questionSetService.GetQuestionsetByVersion(version);

        // Get the ModificationRanking childs from the question set
        var rankings = activeQuestionSet?.Children<ModificationRanking>();

        // get the modification ranking of type Categorisation from the rankings
        var reviewTypes =
            from ranking in rankings
            let children = ranking.Children<ReviewType>()
            where children?.Any() is true
            from child in children
            select child;

        // If no ranking or no ReviewType children, return empty string
        if (reviewTypes?.Any() is false)
        {
            return Task.FromResult(string.Empty);
        }

        // Find the ReviewType that matches the criteria
        var reviewRanking =
        (
            from reviewType in reviewTypes
            from c in reviewType.Criteria!
            let criteria = c.Content as ReviewTypeCriteria
            where criteria.SpecificAreaOfChange?.Any(x => x.Key.ToString() == specificAreaOfChangeId.ToString()) is true
            select reviewType
        ).SingleOrDefault();

        // Return the Value property if found, otherwise empty string
        return Task.FromResult(reviewRanking?.Value ?? string.Empty);
    }
}