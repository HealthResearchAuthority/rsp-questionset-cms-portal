using Rsp.QuestionSetPortal.Models;
using Rsp.QuestionSetService.Models.UIContent;

namespace Rsp.QuestionSetService.Models
{
    public class SectionModel : BaseContentObject
    {
        public string? SectionName { get; set; }

        /// <summary>
        /// Manually assigned by a CMS user
        /// </summary>
        public string? Id { get; set; } = null!;

        public string? CategoryId { get; set; }

        /// <summary>
        /// Contains the questions in this section
        /// </summary>
        public IList<QuestionModel> Questions { get; set; } = new List<QuestionModel>();

        /// <summary>
        /// Contains UI content components in this section
        /// </summary>
        public IList<ContentComponent> GuidanceComponents { get; set; } = [];
    }
}