using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Controllers;

[ApiController]
[Route("/umbraco/api/siteSettings")]
public class ContentSiteSettingsController : ControllerBase
{
    private readonly IPublishedContentQuery _contentQuery;
    private readonly IApiContentResponseBuilder _responseBuilder;

    public ContentSiteSettingsController(IPublishedContentQuery contentQuery,
        IApiContentResponseBuilder responseBuilder)
    {
        _contentQuery = contentQuery;
        _responseBuilder = responseBuilder;
    }

    [HttpGet("getsitesettings")]
    public IApiContentResponse? GetSiteFotter()
    {
        var homeNode = _contentQuery.ContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == Home.ModelTypeAlias);

        if (homeNode != null)
        {
            return _responseBuilder.Build(homeNode);
        }

        return null;
    }
}