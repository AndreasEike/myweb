namespace myweb.api.Models;

public class AnswerKeyEntry
{
    public int MatchQuestionId { get; set; }
    public required string CorrectAnswer { get; set; }
}
