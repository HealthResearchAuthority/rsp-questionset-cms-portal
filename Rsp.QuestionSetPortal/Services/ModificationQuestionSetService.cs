using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Services;

public class ModificationQuestionSetService(IPublishedContentQuery contentQuery) : IModificationQuestionSetService
{
    public ModificationsQuestionSet? GetQuestionsetByVersion(string? version = null)
    {
        var questionsetRepo = contentQuery.ContentAtRoot()?
            .FirstOrDefault(x => x.ContentType.Alias == Questionsets.ModelTypeAlias)?
            .Descendant<ModificationsQuestionsetRepository>();

        if (questionsetRepo != null)
        {
            if (string.IsNullOrEmpty(version))
            {
                // get active questionset because version is not specified
                return questionsetRepo.Value<IPublishedContent>("activeQuestionset") as ModificationsQuestionSet;
            }
            else
            {
                // version is specified so get questionset by version
                var questionset = questionsetRepo
                    .Children<ModificationsQuestionSet>()?
                    .FirstOrDefault(x =>
                        x.VersionNumber
                        .ToString()
                        .Equals(version, StringComparison.InvariantCultureIgnoreCase)
                    );

                return questionset;
            }
        }

        // questionset repository does not exist
        return null;
    }
}