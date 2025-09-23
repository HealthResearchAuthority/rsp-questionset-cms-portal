using Rsp.QuestionSetPortal.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Notifications;

public class PreviewNotificationHandler(IPublishedSnapshotAccessor publishedSnapshotAccessor,
    AppSettings appSettings) : INotificationHandler<SendingContentNotification>
{
    private readonly string[] ContentTypes = {
        GenericPage.ModelTypeAlias,
        MixedContentPage.ModelTypeAlias,
        Home.ModelTypeAlias
    };

    /// <summary>
    /// Generate additonal custom preview URLs for specific page types
    /// </summary>
    public void Handle(SendingContentNotification notification)
    {
        foreach (ContentVariantDisplay variantDisplay in notification.Content.Variants)
        {
            if (!ContentTypes.Contains(notification.Content.ContentTypeAlias))
            {
                continue;
            }

            // get the Portal URL from appsettings
            var portalUrl = appSettings.PortalUrl != null ?
                appSettings.PortalUrl.AbsoluteUri :
                "";

            // Retrieve the route of each content item
            IPublishedSnapshot publishedSnapshot = publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
            var route = publishedSnapshot.Content?.GetRouteById(true, notification.Content.Id);
            if (route == null)
            {
                continue;
            }

            route = route.TrimStart('/');

            if (notification.Content.ContentTypeAlias == Home.ModelTypeAlias)
            {
                // if the page previewed is homepage then we don't want to use any path in the URL
                route = "";
            }

            variantDisplay.AdditionalPreviewUrls = new[]
            {
                new NamedUrl
                {
                    // Dynamically generate Preview URL
                    Name = $"HRA Save & Preview",
                    Url = $"{portalUrl}{route}?preview=true"
                }
            };
        }
    }
}