namespace myweb.api.Models;

public class MatchQuestionResponse
{
    public int MatchQuestionId { get; set; }
    public int QuestionId { get; set; }
    public int OrderIndex { get; set; }
    public required string Text { get; set; }
    public required string ResolvedText { get; set; }
    public QuestionType Type { get; set; }
    public bool HasWildcard { get; set; }
    public string? WildcardValue { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsAnnulled { get; set; }
}
