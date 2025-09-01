using Microsoft.AspNetCore.Mvc;
using Rsp.QuestionSetPortal.Models.WebsiteContent;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Controllers;

[ApiController]
[Route("/umbraco/api/siteSettings")]
public class ContentSiteSettingsController : ControllerBase
{
    private readonly IPublishedContentQuery _contentQuery;

    public ContentSiteSettingsController(IPublishedContentQuery contentQuery)
    {
        _contentQuery = contentQuery;
    }

    [HttpGet("getSiteSettings")]
    public SiteSettingsModel GetSiteFooterAndNavigation()
    {
        var model = new SiteSettingsModel();

        var homeNode = _contentQuery.ContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == Home.ModelTypeAlias) as Home;

        if (homeNode != null)
        {
            var footer = homeNode.FooterLinks?.Select(x =>
               new Link
               {
                   Url = x.Url,
                   Name = x.Name,
                   Target = x.Target
               });

            model.FooterLinks = footer?.ToList();
        }

        return model;
    }
}