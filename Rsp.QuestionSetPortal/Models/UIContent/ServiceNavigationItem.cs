using Umbraco.Cms.Core.Models;

namespace Rsp.QuestionSetPortal.Models.UIContent;

public class ServiceNavigationItemModel
{
    public bool RequiresAuthorisation { get; set; }
    public Link? Link { get; set; }
}