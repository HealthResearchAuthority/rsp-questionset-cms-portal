using Rsp.QuestionSetPortal.Models.UIContent;

namespace Rsp.QuestionSetPortal.Models;

public class SectionModel : BaseContentObject
{
    public string? SectionName { get; set; }

    /// <summary>
    /// Manually assigned by a CMS user
    /// </summary>
    public string? Id { get; set; } = null!;

    public string? CategoryId { get; set; }

    public string? StaticViewName { get; set; }

    public bool IsMandatory { get; set; }

    public int Sequence { get; set; }

    public bool IsLastSectionBeforeReview { get; set; }

    public bool StoreUrlReferrer { get; set; }

    public bool EvaluateBackRoute { get; set; }

    /// <summary>
    /// Contains the questions in this section
    /// </summary>
    public IList<QuestionModel> Questions { get; set; } = new List<QuestionModel>();

    /// <summary>
    /// Contains UI content components in this section
    /// </summary>
    public IList<ContentComponent> GuidanceComponents { get; set; } = [];
}