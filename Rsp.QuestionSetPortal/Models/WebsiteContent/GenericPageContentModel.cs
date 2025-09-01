using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Rsp.QuestionSetPortal.Models.WebsiteContent;

public class GenericPageContentModel
{
    public IApiContentResponse? PageContent { get; set; }
    public IList<Link>? FooterLinks { get; set; }
}