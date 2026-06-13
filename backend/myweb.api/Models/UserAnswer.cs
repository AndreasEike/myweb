namespace myweb.api.Models;

public class UserAnswer
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int MatchQuestionId { get; set; }
    public MatchQuestion MatchQuestion { get; set; } = null!;
    public required string Answer { get; set; }
    public bool? IsCorrect { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
