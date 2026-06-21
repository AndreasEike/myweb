namespace myweb.api.Models;

public class AnswerKeyEntry
{
    public int MatchQuestionId { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsAnnulled { get; set; }
}
