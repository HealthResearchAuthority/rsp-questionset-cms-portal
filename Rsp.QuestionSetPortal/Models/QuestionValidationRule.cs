namespace Rsp.QuestionSetPortal.Models;

public class QuestionValidationRule
{
    public int MaximumCharacters { get; set; }
    public int MinimumCharacters { get; set; }
    public bool DigitsOnly { get; set; }
    public bool EmailFormat { get; set; }
}