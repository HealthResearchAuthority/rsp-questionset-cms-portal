using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Notifications;

public class MovingToRecycleBinNotifications(IPublishedContentQuery contentService) : INotificationHandler<ContentMovingToRecycleBinNotification>
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

    public void Handle(ContentMovingToRecycleBinNotification notification)
    {
        foreach (var node in notification.MoveInfoCollection)
        {
            if (!TypeAlias.Contains(node.Entity.ContentType.Alias))
            {
                return;
            }
            var savingNode = contentService.Content(node.Entity.Id);

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
                    notification.CancelOperation(new EventMessage("Deleting is not allowed",
                    "This Questionset is in <b>" + status + "</b> mode. Please create a new version of this questionset to make changes.",
                    EventMessageType.Warning));
                }
            }
        }
    }
}