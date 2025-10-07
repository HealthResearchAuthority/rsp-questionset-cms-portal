using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.Services;

public interface IModificationQuestionSetService
{
    ModificationsQuestionSet? GetQuestionsetByVersion(string? version = null);
}