using Rsp.QuestionSetPortal.Models.UIContent;

namespace Rsp.QuestionSetPortal.Models;

public class QuestionModel : BaseContentObject
{
    public string? Name { get; set; }
    public string? ShortName { get; set; }

    /// <summary>
    /// Manually created Id by the user
    /// </summary>
    public string? Id { get; set; } = null!;

    public string? Label { get; set; }
    public string? Conformance { get; set; }
    public string? QuestionFormat { get; set; }
    public string? AnswerDataType { get; set; }
    public string? CategoryId { get; set; }
    public bool ShowOriginalAnswer { get; set; }
    public int Sequence { get; set; }
    public int SectionSequence { get; set; }

    /// <summary>
    /// Represents the version of the questionset
    /// </summary>
    public string? Version { get; set; }

    public IList<AnswerModel> Answers { get; set; } = [];
    public IList<RuleModel> ValidationRules { get; set; } = [];
    public IList<ContentComponent> GuidanceComponents { get; set; } = [];
    public string? ShowAnswerOn { get; set; }
    public string? SectionGroup { get; set; }
    public int SequenceInSectionGroup { get; set; }

    // properties for calculating ranking
    public string? NhsInvolvment { get; set; }

    public string? NonNhsInvolvment { get; set; }
    public bool AffectedOrganisations { get; set; }
    public bool RequireAdditionalResources { get; set; }
    public bool UseAnswerForNextSection { get; set; }
}