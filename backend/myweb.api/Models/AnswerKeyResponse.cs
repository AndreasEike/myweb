namespace myweb.api.Models;

public class AnswerKeyResponse
{
    public int QuestionsKeyed { get; set; }
    public int ScoredAnswers { get; set; }
    public int Participants { get; set; }
    public bool Published { get; set; }
}
