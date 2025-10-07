using Microsoft.AspNetCore.Mvc;
using Rsp.QuestionSetPortal.Models.UIContent;
using Rsp.QuestionSetPortal.Models.WebsiteContent;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Controllers;

[ApiController]
[Route("/umbraco/api/siteSettings")]
public class ContentSiteSettingsController : ControllerBase
{
    private readonly IUmbracoContextAccessor _contentQuery;

    public ContentSiteSettingsController(IUmbracoContextAccessor contentQuery)
    {
        _contentQuery = contentQuery;
    }

    [HttpGet("getSiteSettings")]
    [ProducesResponseType(typeof(SiteSettingsModel), StatusCodes.Status200OK)]
    public IActionResult GetSiteFooterAndNavigation(bool preview = false)
    {
        var model = new SiteSettingsModel();

        var tryContext = _contentQuery.TryGetUmbracoContext(out var umbC);

        if (!tryContext)
        {
            return BadRequest("UmbracoContext could not be instantiated.");
        }

        var homeNode = umbC?.Content?.GetAtRoot(preview: preview)?.FirstOrDefault(x => x.ContentType.Alias == Home.ModelTypeAlias) as Home;

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

            if (homeNode.PhaseBannerContent != null)
            {
                model.PhaseBannerContent = new RichTextProperty
                {
                    Value = new RichTextValue
                    {
                        Markup = homeNode.PhaseBannerContent.ToHtmlString()
                    }
                };
            }
        }

        return Ok(model);
    }
}