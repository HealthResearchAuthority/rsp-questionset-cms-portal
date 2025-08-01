using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.CMS_Extensions;

public class MemberComposer() : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<MenuRenderingNotification, MemberComponent>();
    }
}

public class MemberComponent(IContentService contentService) : INotificationHandler<MenuRenderingNotification>
{
    public void Handle(MenuRenderingNotification notification)
    {
        if (notification.TreeAlias != "content")
        {
            return;
        }

        var content = contentService.GetById(int.Parse(notification.NodeId));

        if (content != null)
        {
            if (content.ContentType.Alias == QuestionSet.ModelTypeAlias)
            {
                var m = new MenuItem("createNewVersion", "Create new version");
                m.LaunchDialogView("/App_Plugins/CreateNewVersion/dialog.html", "Create new version");
                m.AdditionalData.Add("nodeId", content.Key);
                m.Icon = "tactics";
                notification.Menu.Items.Insert(3, m);
            }
        }
    }
}