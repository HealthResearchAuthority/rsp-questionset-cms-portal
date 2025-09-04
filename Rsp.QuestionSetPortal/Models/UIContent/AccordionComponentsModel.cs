namespace Rsp.QuestionSetPortal.Models.UIContent;

public class AccordionComponentModel : ContentComponent
{
    public IList<AccordionComponentItemModel> Items { get; set; } = [];
}

public class AccordionComponentItemModel
{
    public string? Title { get; set; }
    public string? Value { get; set; }
}