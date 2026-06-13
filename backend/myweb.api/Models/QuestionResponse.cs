namespace myweb.api.Models;

public class QuestionResponse
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public QuestionType Type { get; set; }
    public bool HasWildcard { get; set; }
    public int UsageCount { get; set; }
}
