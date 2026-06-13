namespace myweb.api.Models;

public class SubmitAnswerEntry
{
    public int MatchQuestionId { get; set; }
    public required string Answer { get; set; }
}
