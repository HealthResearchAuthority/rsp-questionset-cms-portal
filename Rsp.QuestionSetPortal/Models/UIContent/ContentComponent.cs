using System.Text.Json.Serialization;

namespace Rsp.QuestionSetPortal.Models.UIContent;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(AccordionComponentModel))]
[JsonDerivedType(typeof(DetailsComponentModel))]
[JsonDerivedType(typeof(TabsComponentModel))]
[JsonDerivedType(typeof(BodyTextComponentModel))]
public class ContentComponent
{
    public string ContentType { get; set; } = null!;
}