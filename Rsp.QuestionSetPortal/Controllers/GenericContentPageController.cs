using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Controllers;

[ApiController]
[Route("/umbraco/api/genericContentPage")]
public class GenericContentPageController : ControllerBase
{
    private readonly IUmbracoContextAccessor _contentQuery;
    private readonly IApiContentResponseBuilder _responseBuilder;

    public GenericContentPageController(IUmbracoContextAccessor contentQuery,
         IApiContentResponseBuilder responseBuilder)
    {
        _contentQuery = contentQuery;
        _responseBuilder = responseBuilder;
    }

    [HttpGet("getByUrl")]
    [ProducesResponseType(typeof(IApiContentResponse), StatusCodes.Status200OK)]
    public IActionResult GetContent(string url)
    {
        url = url.StartsWith('/') ? url : "/" + url;
        var tryContext = _contentQuery.TryGetUmbracoContext(out var umbC);

        if (!tryContext)
        {
            return BadRequest("UmbracoContext could not be instantiated.");
        }

        var contentItem = umbC?.Content?.GetByRoute(url);
        if (contentItem == null)
        {
            return NotFound();
        }

        var pageResponse = _responseBuilder.Build(contentItem);

        return Ok(pageResponse);
    }

    [HttpGet("getHomeContent")]
    [ProducesResponseType(typeof(IApiContentResponse), StatusCodes.Status200OK)]
    public IActionResult GetHomeContent()
    {
        var tryContext = _contentQuery.TryGetUmbracoContext(out var umbC);

        if (!tryContext)
        {
            return BadRequest("UmbracoContext could not be instantiated.");
        }

        var contentItem = umbC?.Content?.GetAtRoot()?.FirstOrDefault(x => x.ContentType.Alias == Home.ModelTypeAlias)
            ;
        if (contentItem == null)
        {
            return NotFound();
        }

        var pageResponse = _responseBuilder.Build(contentItem);

        return Ok(pageResponse);
    }
}