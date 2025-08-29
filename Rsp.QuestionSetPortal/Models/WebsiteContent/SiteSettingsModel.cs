using Umbraco.Cms.Core.Models;

namespace Rsp.QuestionSetPortal.Models.WebsiteContent;

public class SiteSettingsModel
{
    public IList<Link>? FooterLinks { get; set; }
    public object? LoginLandingPageContent { get; set; }
}