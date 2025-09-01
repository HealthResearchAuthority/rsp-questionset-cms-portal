using Microsoft.AspNetCore.Mvc;
using Rsp.QuestionSetPortal.Models.WebsiteContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Controllers;

[ApiController]
[Route("/umbraco/api/mixedContentPage")]
public class MixedContentPageController : ControllerBase
{
    private readonly IUmbracoContextAccessor _contentQuery;

    public MixedContentPageController(IUmbracoContextAccessor contentQuery)
    {
        _contentQuery = contentQuery;
    }

    [HttpGet("getByUrl")]
    public MixedContentPageModel? GetContent(string url)
    {
        url = url.StartsWith('/') ? url : "/" + url;
        var model = new MixedContentPageModel();

        var tryContext = _contentQuery.TryGetUmbracoContext(out var umbC);

        if (!tryContext)
        {
            return null;
        }

        MixedContentPage? contentItem = umbC?.Content?.GetByRoute(url) as MixedContentPage;

        if (contentItem == null)
        {
            return null;
        }

        var contentItems = contentItem.ContentItems;

        if (contentItems == null)
        {
            return null;
        }

        foreach (var placeholderItem in contentItems.Select(x => x.Content as MixedPagePlaceholderItem))
        {
            if (placeholderItem != null)
            {
                var placeholderNode = placeholderItem.Placeholder;
                var value = placeholderItem.Content;

                if (placeholderNode != null)
                {
                    model.ContentItems.Add(placeholderNode.Name, value);
                }
            }
        }

        return model;
    }
}