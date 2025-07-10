namespace Rsp.QuestionSetService.Models.UIContent
{
    public class TabsComponentModel : ContentComponent
    {
        public IList<TabComponentItemModel> Items { get; set; } = [];
    }

    public class TabComponentItemModel
    {
        public string? Title { get; set; }
        public string? Value { get; set; }
    }
}