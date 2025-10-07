﻿namespace Rsp.QuestionSetPortal.DTOs.Requests;

public class RankingOfChangeRequest
{
    public Guid SpecificAreaOfChangeId { get; set; } = Guid.Empty;
    public string Applicability { get; set; } = null!;
    public string ProjectType { get; set; } = "non-REC";
    public bool IsNHSInvolved { get; set; } = false;
    public bool IsNonNHSInvolved { get; set; } = false;
    public string? NhsOrganisationsAffected { get; set; }
    public bool NhsResourceImplicaitons { get; set; } = false;
    public string? Version { get; set; }
}