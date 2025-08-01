using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Rsp.QuestionSetPortal.CMS_Extensions;

public class CreateNewVersionApiController : UmbracoAuthorizedApiController
{
    private readonly IContentService _contentService;

    public CreateNewVersionApiController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpPost]
    public IActionResult DeepCopyWithRelink([FromBody] DeepCopyRequest request)
    {
        var original = _contentService.GetById(request.Id);
        if (original == null) return NotFound();

        var idMap = new Dictionary<string, CopyDocumentIds>();

        // Recursive deep copy
        var newRoot = CopyContentRecursive(original, original.ParentId, idMap);

        // Relink internal references
        foreach (var kvp in idMap)
        {
            var newItem = _contentService.GetById(kvp.Value.Guid);
            if (newItem == null) continue;

            foreach (var prop in newItem.Properties)
            {
                var editor = prop.PropertyType.PropertyEditorAlias;

                if (editor == "Umbraco.ContentPicker")
                {
                    var val = prop.GetValue() as string;
                    if (idMap.TryGetValue(val, out var newLinkedId))
                    {
                        prop.SetValue(newLinkedId.ToString());
                    }
                }
                else if (editor == "Umbraco.MultiNodeTreePicker")
                {
                    var val = prop.GetValue() as string;
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        var ids = val.Split(',').Select(v => v).ToList();
                        var newIds = ids.Select(id => idMap.TryGetValue(id, out var newId) ? newId.GuidUdi.ToString() : id).ToList();
                        prop.SetValue(string.Join(",", newIds));
                    }
                }
                else if (editor == "Umbraco.BlockList")
                {
                    var val = prop.GetValue() as string;
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        try
                        {
                            foreach (var kv in idMap)
                            {
                                //var originalUdi = Udi.Create("document", Guid.Parse(kv.Key)).UriValue.ToString();
                                //var newUdi = Udi.Create("document", Guid.Parse(kv.Value)).UriValue.ToString();

                                //var findContent = val.IndexOf(kv.Key);

                                val = val.Replace(kv.Key, kv.Value.GuidUdi.ToString(), StringComparison.InvariantCultureIgnoreCase);
                            }

                            prop.SetValue(val);
                        }
                        catch (Exception ex)
                        {
                            // Optional: log block parsing error
                        }
                    }
                }
            }

            _contentService.SaveAndPublish(newItem);
        }

        return Ok(new { newId = newRoot.Id });
    }

    private IContent CopyContentRecursive(IContent source, int parentId, Dictionary<string, CopyDocumentIds> idMap)
    {
        var newName = source.ContentType.Alias == QuestionSet.ModelTypeAlias ?
            source.Name + " New version" :
            source.Name;

        var copy = _contentService.Create(newName, parentId, source.ContentType.Alias);

        foreach (var prop in source.Properties)
        {
            copy.SetValue(prop.Alias, prop.GetValue());
        }

        // this is the version property so let's update it with a new version
        if (copy.ContentType.Alias == QuestionSet.ModelTypeAlias)
        {
            var dropdownValue = JsonConvert.SerializeObject(new[] { "Draft" });
            var oldVersion = source.GetValue<int>("versionNumber");
            copy.SetValue("versionNumber", oldVersion + 1);
            copy.SetValue("status", dropdownValue);
        }

        var published = _contentService.SaveAndPublish(copy);
        if (!published.Success)
        {
            var res = published;
        }

        // convert document GUID into Umbraco's UDI
        var originalDocumentUdi = source.GetUdi().ToString();
        var copyDocumentUdi = copy.GetUdi();

        idMap[originalDocumentUdi] = new CopyDocumentIds
        {
            GuidUdi = copyDocumentUdi,
            Guid = copy.Key
        };

        var children = _contentService.GetPagedChildren(source.Id, 0, int.MaxValue, out _);
        foreach (var child in children)
        {
            CopyContentRecursive(child, copy.Id, idMap);
        }

        return copy;
    }

    public class DeepCopyRequest
    {
        public int Id { get; set; }
    }

    public class CopyDocumentIds
    {
        public Guid Guid { get; set; }
        public GuidUdi GuidUdi { get; set; } = null!;
    }
}