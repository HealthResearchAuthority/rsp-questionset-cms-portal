using Rsp.QuestionSetPortal.Models.UIContent;
using Umbraco.Cms.Core.Models;

namespace Rsp.QuestionSetPortal.Models.WebsiteContent;

public class SiteSettingsModel
{
    public IList<Link>? FooterLinks { get; set; }
    public RichTextProperty? PhaseBannerContent { get; set; }
    public IList<ServiceNavigationItemModel>? ServiceNavigation { get; set; }
    public RichTextProperty? CookieBannerContent { get; set; }
}