using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Notifications;

public class PublishingNotifications(
    IPublishedContentQuery contentService,
    IBackOfficeSecurity backofficeSecutiry) : INotificationHandler<ContentPublishingNotification>
{
    // execute this notification when saving the following content types
    private readonly string[] TypeAlias =
        [
            QuestionSet.ModelTypeAlias,
            Question.ModelTypeAlias,
            Section.ModelTypeAlias,
            AnswerOption.ModelTypeAlias,
            AnswerSet.ModelTypeAlias
        ];

    public void Handle(ContentPublishingNotification notification)
    {
        // get the current CMS user
        var currentCmsUser = backofficeSecutiry.CurrentUser;

        // check if CMS user is admin in which case let them delete the content
        if (currentCmsUser != null && currentCmsUser.IsAdmin())
        {
            return;
        }

        foreach (var node in notification.PublishedEntities)
        {
            if (!TypeAlias.Contains(node.ContentType.Alias))
            {
                return;
            }

            var savingNode = contentService.Content(node.Id);

            if (savingNode == null)
            {
                return;
            }

            var questionSet = savingNode.AncestorOrSelf<QuestionSet>();

            if (questionSet != null)
            {
                var status = questionSet.Status;

                if (status == "Published")
                {
                    // questionset is already poublished or live so no saving allowed
                    notification.CancelOperation(new EventMessage("Editing is not allowed",
                    "This Questionset is in <b>" + status + "</b> mode. Please create a new version of this questionset to make changes.",
                    EventMessageType.Warning));
                }
            }
        }
    }
}