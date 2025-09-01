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
    public IApiContentResponse? GetContent(string url)
    {
        url = url.StartsWith('/') ? url : "/" + url;
        var tryContext = _contentQuery.TryGetUmbracoContext(out var umbC);

        if (!tryContext)
        {
            return null;
        }

        var contentItem = umbC?.Content?.GetByRoute(url);
        if (contentItem == null)
        {
            return null;
        }

        var pageResponse = _responseBuilder.Build(contentItem);

        return pageResponse;
    }

    [HttpGet("getHomeContent")]
    public IApiContentResponse? GetHomeContent()
    {
        var tryContext = _contentQuery.TryGetUmbracoContext(out var umbC);

        if (!tryContext)
        {
            return null;
        }

        var contentItem = umbC?.Content?.GetAtRoot()?.FirstOrDefault(x => x.ContentType.Alias == Home.ModelTypeAlias)
            ;
        if (contentItem == null)
        {
            return null;
        }

        var pageResponse = _responseBuilder.Build(contentItem);

        return pageResponse;
    }
}