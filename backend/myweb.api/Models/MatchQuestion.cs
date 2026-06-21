namespace myweb.api.Models;

public class MatchQuestion
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public int OrderIndex { get; set; }
    public string? WildcardValue { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsAnnulled { get; set; }
    public List<UserAnswer> Answers { get; set; } = [];
}
