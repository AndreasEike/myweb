namespace myweb.api.Models;

public class Question
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public QuestionType Type { get; set; }
    public bool HasWildcard { get; set; }
}
