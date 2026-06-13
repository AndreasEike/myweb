namespace myweb.api.Models;

public class QuestionRequest
{
    public required string Text { get; set; }
    public QuestionType Type { get; set; }
    public bool HasWildcard { get; set; }
}
