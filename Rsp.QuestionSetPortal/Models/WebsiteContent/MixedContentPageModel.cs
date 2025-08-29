namespace Rsp.QuestionSetPortal.Models.WebsiteContent;

public class MixedContentPageModel
{
    public IDictionary<string, string?> ContentItems { get; set; } = new Dictionary<string, string?>();
}

public class MixedContentPageItem
{
    public string? ContentPlaceholderAlias { get; set; }
    public string? ContentValue { get; set; }
}