namespace myweb.api.Models;

public class QuizQuestionResponse
{
    public int MatchQuestionId { get; set; }
    public int OrderIndex { get; set; }
    public required string Text { get; set; }
    public QuestionType Type { get; set; }
    public string? MyAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool? IsCorrect { get; set; }
}
