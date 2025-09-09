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
    [ProducesResponseType(typeof(MixedContentPageModel), StatusCodes.Status200OK)]
    public IActionResult GetContent(string url)
    {
        url = url.StartsWith('/') ? url : "/" + url;
        var model = new MixedContentPageModel();

        var tryContext = _contentQuery.TryGetUmbracoContext(out var umbC);

        if (!tryContext)
        {
            return BadRequest("UmbracoContext could not be instantiated.");
        }

        MixedContentPage? contentItem = umbC?.Content?.GetByRoute(url) as MixedContentPage;

        if (contentItem == null)
        {
            return NotFound();
        }

        var contentItems = contentItem.ContentItems;

        if (contentItems != null)
        {
            foreach (var placeholderItem in contentItems.Select(x => x.Content as MixedPagePlaceholderItem))
            {
                if (placeholderItem?.Placeholder != null)
                {
                    var placeholderNode = placeholderItem.Placeholder;
                    var value = placeholderItem?.Content?.FirstOrDefault();

                    if (value != null)
                    {
                        var valueType = value.Content.ContentType.Alias;
                        var placeholderValue = value.Content.Value<string>("value");

                        model.ContentItems.TryAdd(placeholderNode.Name, new MixedContentPageItem
                        {
                            Value = placeholderValue,
                            ValueType = valueType
                        });
                    }
                }
            }
        }

        return Ok(model);
    }
}